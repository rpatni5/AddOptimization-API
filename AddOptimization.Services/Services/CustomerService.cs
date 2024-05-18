using Microsoft.Extensions.Logging;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Contracts.Constants;
using AddOptimization.Utilities.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using AddOptimization.Utilities.Enums;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Interface;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Services;
using Microsoft.Extensions.Configuration;
using System;
using iText.StyledXmlParser.Css.Selector.Item;

namespace AddOptimization.Services.Services;
public class CustomerService : ICustomerService
{
    private readonly IGenericRepository<Customer> _customerRepository;
    private readonly IGenericRepository<ApplicationUser> _applicationUserRepository;
    private readonly IGenericRepository<CustomerStatus> _customerStatusRepository;
    private readonly ILogger<CustomerService> _logger;
    private readonly IMapper _mapper;
    private readonly List<string> _currentUserRoles;
    private readonly IAddressService _addressService;
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly IConfiguration _configuration;
    private readonly IGenericRepository<PasswordResetToken> _passwordResetTokenRepository;
    private readonly IGenericRepository<Role> _roleRepository;
    private readonly IGenericRepository<UserRole> _userRoleRepository;


    private readonly IUnitOfWork _unitOfWork;
    public CustomerService(IGenericRepository<Customer> customerRepository, ILogger<CustomerService> logger, IMapper mapper,
        IAddressService addressService, IUnitOfWork unitOfWork, IEmailService emailService, ITemplateService templateService,
        IGenericRepository<PasswordResetToken> passwordResetTokenRepository, IGenericRepository<CustomerStatus> customerStatusRepository,
        IGenericRepository<UserRole> userRoleRepository,
        IGenericRepository<Role> roleRepository,
        IGenericRepository<ApplicationUser> applicationUserRepository, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _customerRepository = customerRepository;
        _applicationUserRepository = applicationUserRepository;
        _logger = logger;
        _mapper = mapper;
        _addressService = addressService;
        _unitOfWork = unitOfWork;
        _customerStatusRepository = customerStatusRepository;
        _currentUserRoles = httpContextAccessor.HttpContext.GetCurrentUserRoles();
        _emailService = emailService;
        _templateService = templateService;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
    }
    public async Task<ApiResult<List<CustomerSummaryDto>>> GetSummary(PageQueryFiterBase filter)
    {
        try
        {
            var includeDeleted = false;
            filter.GetValue<bool>("includeInactive", v => includeDeleted = v);
            var entities = await _customerRepository.QueryMappedAsync(s => new CustomerSummaryDto
            {
                Id = s.Id,
                Name = s.Name,
            }, e => includeDeleted || (e.CustomerStatus != null && e.CustomerStatus.Name != CustomerStatuses.Inactive), orderBy: (entities) => entities.OrderBy(c => c.Name), ignoreGlobalFilter: true);
            return ApiResult<List<CustomerSummaryDto>>.Success(entities.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    public async Task<PagedApiResult<CustomerDto>> Search(PageQueryFiterBase filter)
    {
        try
        {
            var superAdminRole = _currentUserRoles.Where(c => c.Contains("Super Admin")).ToList();
            var entities = await _customerRepository.QueryAsync(include: entities => entities
            .Include(e => e.CustomerStatus).Include(e => e.Licenses).Include(e => e.BillingAddress).Include(e => e.Country), orderBy: (entities) => entities.OrderBy(t => t.Name), ignoreGlobalFilter: superAdminRole.Count != 0);

            entities = ApplySorting(entities, filter?.Sorted?.FirstOrDefault());
            entities = ApplyFilters(entities, filter);
            var pagedResult = PageHelper<Customer, CustomerDto>.ApplyPaging(entities, filter, entities => entities.Select(e => new CustomerDto
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.Email,
                BirthDay = e.Birthday,
                Company = e.Organizations,
                Notes = e.Notes,
                Phone = e.Phone,
                CustomerStatusId = e.CustomerStatusId,
                ContactInfo = e.ContactInfo,
                Licenses = _mapper.Map<List<LicenseDetailsDto>>(e.Licenses),
                CountryCode = e.CountryCode,
                CustomerStatusName = e.CustomerStatus.Name,
                BillingAddressString = e.BillingAddress == null ? null : $"{e.BillingAddress.Address1},{e.BillingAddress.Zip},{e.BillingAddress.City}",
                ManagerName = e.ManagerName,
                VAT = e.VAT,
                PaymentClearanceDays = e.PaymentClearanceDays,
                CountryId = e.CountryId,
                IsApprovalRequired = e.IsApprovalRequired,
                PartnerName = e.PartnerName,
                PartnerBankName = e.PartnerBankName,
                PartnerBankAccountName = e.PartnerBankAccountName,
                PartnerBankAccountNumber = e.PartnerBankAccountNumber,
                PartnerCountryId = e.PartnerCountryId,
                PartnerPostalCode = e.PartnerPostalCode,
                PartnerAddress = e.PartnerAddress,
                PartnerDescriptions = e.PartnerDescriptions,

            }).ToList());
            var retVal = pagedResult;
            return PagedApiResult<CustomerDto>.Success(retVal);
            //var mappedEntities = _mapper.Map<List<CustomerDto>>(entities.ToList());
            //return ApiResult<List<CustomerDto>>.Success(mappedEntities);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    public async Task<ApiResult<CustomerDetailsDto>> Get(Guid id, bool includeOrderStats)
    {
        try
        {
            var entity = await _customerRepository.FirstOrDefaultAsync(t => t.Id == id, include: entity => entity.Include(e => e.Addresses.Where(a => !a.IsDeleted).OrderByDescending(e => e.CreatedAt)).Include(e => e.CustomerStatus), ignoreGlobalFilter: true);
            if (entity == null)
            {
                return ApiResult<CustomerDetailsDto>.NotFound("Customer");
            }
            var mappedEntity = _mapper.Map<CustomerDetailsDto>(entity);
            if (includeOrderStats)
            {
                //var ordersData = (await _orderRepository.QueryMappedAsync(e => new {
                //    e.CreatedAt,
                //    OrderAmount = e.Totals == null ? 0 : e.Totals.total
                //}, e => e.CustomerId == id)).ToList();
                //if (ordersData.Any())
                //{
                //    mappedEntity.AverageOrderAmount = (int)Math.Ceiling((double)(ordersData.Sum(e => e.OrderAmount) / ordersData.Count));
                //    mappedEntity.OrderCount = ordersData.Count;
                //    mappedEntity.LastOrdered = ordersData.OrderByDescending(e => e.CreatedAt).FirstOrDefault()?.CreatedAt;
                //}
            }
            return ApiResult<CustomerDetailsDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<CustomerDto>> Create(CustomerCreateDto model)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var isExists = await _customerRepository.IsExist(t => t.Email.ToLower() == model.Email.ToLower(), ignoreGlobalFilter: true);
            var isExistInAppUser = await _applicationUserRepository.IsExist(t => t.Email.ToLower() == model.Email.ToLower(), ignoreGlobalFilter: true);

            if (isExists || isExistInAppUser)
            {
                var errorMessage = isExistInAppUser ? "User already exists with some other role in the system." : "Customer already exists with same email.";
                return ApiResult<CustomerDto>.Failure(ValidationCodes.EmailUserNameAlreadyExists, errorMessage);
            }
            var entity = _mapper.Map<Customer>(model);
            var billingAddressId = entity.BillingAddressId;
            entity.BillingAddressId = null;
            entity = await _customerRepository.InsertAsync(entity);
            if (model.Addresses.Any())
            {
                model.Addresses.ForEach(a =>
                {
                    a.CustomerId = entity.Id;
                });
                await _addressService.BulkCreate(model.Addresses);
                if (billingAddressId != null)
                {
                    entity.BillingAddressId = billingAddressId;
                    await _customerRepository.UpdateAsync(entity);
                }
            }
            var names = model.Name.Split(' ');
            string firstName = names[0] != null ? names[0] : null;
            string lastName = names.Length > 1 ? names[names.Count() - 1] : null;
            var appUserEntity = new ApplicationUser
            {
                Email = model.Email,
                FirstName = firstName,
                LastName = lastName,
                FullName = model.Name,
                IsEmailsEnabled = true,
                IsActive = true,
                IsLocked = false,
                UserName = model.Email
            };
            var appUserEntityValue = await _applicationUserRepository.InsertAsync(appUserEntity);

            //Get Roles
            var roles = (await _roleRepository.QueryAsync(x => !x.IsDeleted)).ToList();
            var customerRole = roles?.FirstOrDefault(x => x.Name == "Customer" && !x.IsDeleted);
            if (customerRole != null)
            {
                var defaultRole = new UserRole
                {
                    CreatedAt = DateTime.Now,
                    CreatedByUserId = appUserEntityValue.CreatedByUserId,
                    UserId = appUserEntityValue.Id,
                    RoleId = customerRole.Id
                };

                //Add default roles for customer
                await _userRoleRepository.InsertAsync(defaultRole);
            }
            else
            {
                _logger.LogInformation("Unable to find customer role or it is deleted.");
            }

            await _unitOfWork.CommitTransactionAsync();

            var isLimitReached = (await _passwordResetTokenRepository.QueryAsync(e => e.CreatedByUserId == appUserEntityValue.Id && !e.IsExpired
           && e.CreatedAt > DateTime.UtcNow.Date)).Count() >= 3;
            if (isLimitReached)
            {
                return ApiResult<CustomerDto>.Failure(ValidationCodes.PasswordResetLinkLimitReached);
            }
            string token = GenerateToken();
            var resetToken = new PasswordResetToken
            {
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddDays(1),
                CreatedByUserId = appUserEntityValue.Id
            };
            resetToken = await _passwordResetTokenRepository.InsertAsync(resetToken);

            //Send password reset email
            var isEmailSent = await SendCustomerCreatedAndResetPasswordEmail(token, appUserEntityValue.FullName, appUserEntityValue.Email);
            if (!isEmailSent)
            {
                await _passwordResetTokenRepository.DeleteAsync(resetToken);
                return ApiResult<CustomerDto>.Failure(ValidationCodes.IssueSendingEmail);

            }
            var mappedEntity = _mapper.Map<CustomerDto>(entity);
            return ApiResult<CustomerDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<CustomerDto>> Update(Guid id, CustomerCreateDto model)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var isExists = await _customerRepository.IsExist(t => t.Id != id && t.Email.ToLower() == model.Email.ToLower(), ignoreGlobalFilter: true);

            if (isExists)
            {
                var errorMessage = "Customer already exists with same email.";
                return ApiResult<CustomerDto>.Failure(ValidationCodes.EmailUserNameAlreadyExists, errorMessage);
            }

            var entity = await _customerRepository.FirstOrDefaultAsync(t => t.Id == id, ignoreGlobalFilter: true);
            if (entity == null)
            {
                return ApiResult<CustomerDto>.NotFound("Customer");
            }

            //Update application user
            if (entity.Email != model.Email || entity.Name != model.Name)
            {
                var appUserEntityValue = (await _applicationUserRepository.QueryAsync(c => c.Email == entity.Email && c.IsActive)).FirstOrDefault();
                appUserEntityValue.FullName = model.Name;
                appUserEntityValue.Email = model.Email;
                await _applicationUserRepository.UpdateAsync(appUserEntityValue);
            }
            _mapper.Map(model, entity);
            await _customerRepository.UpdateAsync(entity);
            await _unitOfWork.CommitTransactionAsync();

            var mappedEntity = _mapper.Map<CustomerDto>(entity);
            return ApiResult<CustomerDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<bool>> Delete(Guid id)
    {
        try
        {
            var entity = await _customerRepository.FirstOrDefaultAsync(t => t.Id == id, ignoreGlobalFilter: true);
            if (entity == null)
            {
                return ApiResult<bool>.NotFound("Customer");
            }
            var inActiveStatus = await _customerStatusRepository.FirstOrDefaultAsync(e => e.Name == CustomerStatuses.Inactive);
            if (inActiveStatus == null)
            {
                return ApiResult<bool>.NotFound("Customer Status");
            }
            entity.CustomerStatusId = inActiveStatus.Id;
            await _customerRepository.UpdateAsync(entity);
            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    //public async Task<ApiResult<List<CustomerOrderDto>>> GetOrders(Guid customerId)
    //{
    //    try
    //    {
    //        var entities = await _orderRepository.QueryMappedAsync(e=> new CustomerOrderDto
    //        {
    //            Id=e.Id,
    //            InvoiceNumber= e.InvoiceNumber,
    //            Duedate=e.Duedate,
    //            OrderStatusId=e.OrderStatusId,
    //            OrderStatusName=e.OrderStatus.Name,
    //            UserId=e.UserId,
    //            UserFullName=e.UserFullName,
    //            ShippingAddressString = e.ShippingAddress==null?null:e.ShippingAddress.address1+","+e.ShippingAddress.zip + "," + e.ShippingAddress.city,
    //            City = e.ShippingAddress==null?null: e.ShippingAddress.city,
    //            Total = e.Totals==null ? null :e.Totals.total/100
    //        },e=> e.CustomerId==customerId,include: source => source.Include(o => o.OrderStatus),orderBy:entities=> entities.OrderByDescending(o=> o.CreatedAt));

    //        return ApiResult<List<CustomerOrderDto>>.Success(entities.ToList());
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogException(ex);
    //        throw;
    //    }
    //}

    private IQueryable<Customer> ApplyFilters(IQueryable<Customer> entities, PageQueryFiterBase filter)
    {

        filter.GetValue<string>("Name", (v) =>
        {
            entities = entities.Where(e => e.Name != null && e.Name.ToLower().Contains(v.ToLower()));
        });
        filter.GetValue<string>("Email", (v) =>
        {
            entities = entities.Where(e => e.Email != null && e.Email.ToLower().Contains(v.ToLower()));
        });

        filter.GetValue<string>("Phone", (v) =>
        {
            entities = entities.Where(e => e.Phone != null && e.Phone.ToLower().Contains(v.ToLower()));
        });
        filter.GetValue<string>("CustomerStatusId", (v) =>
        {
            if (v != (new Guid()).ToString())
            {
                Guid statusId = new Guid(v);
                entities = entities.Where(e => e.CustomerStatusId == statusId);
            }
        });
        return entities;
    }

    private IQueryable<Customer> ApplySorting(IQueryable<Customer> orders, SortModel sort)
    {
        try
        {
            if (sort?.Name == null)
            {
                orders = orders.OrderByDescending(o => o.CreatedAt);
                return orders;
            }
            var columnName = sort.Name.ToUpper();
            if (sort.Direction == SortDirection.ascending.ToString())
            {
                if (columnName.ToUpper() == nameof(CustomerDto.Name).ToUpper())
                {
                    orders = orders.OrderBy(o => o.Name);
                }
                if (columnName.ToUpper() == nameof(CustomerDto.Email).ToUpper())
                {
                    orders = orders.OrderBy(o => o.Email);
                }
                if (columnName.ToUpper() == nameof(CustomerDto.Phone).ToUpper())
                {
                    orders = orders.OrderBy(o => o.Phone);
                }
                if (columnName.ToUpper() == nameof(CustomerDto.CustomerStatus).ToUpper())
                {
                    orders = orders.OrderBy(o => o.CustomerStatus);
                }

            }
            else
            {
                if (columnName.ToUpper() == nameof(CustomerDto.Name).ToUpper())
                {
                    orders = orders.OrderByDescending(o => o.Name);
                }
                if (columnName.ToUpper() == nameof(CustomerDto.Email).ToUpper())
                {
                    orders = orders.OrderByDescending(o => o.Email);
                }
                if (columnName.ToUpper() == nameof(CustomerDto.Phone).ToUpper())
                {
                    orders = orders.OrderByDescending(o => o.Phone);
                }
                if (columnName.ToUpper() == nameof(CustomerDto.CustomerStatus).ToUpper())
                {
                    orders = orders.OrderByDescending(o => o.CustomerStatus);
                }
            }
            return orders;

        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            return orders;
        }
    }

    private static string GenerateToken()
    {
        byte[] guidBytes = Guid.NewGuid().ToByteArray();
        return Convert.ToBase64String(guidBytes);
    }
    private async Task<bool> SendCustomerCreatedAndResetPasswordEmail(string token, string userFullName, string email)
    {
        try
        {
            var subject = "Create your account Password";
            var emailTemplate = _templateService.ReadTemplate(EmailTemplates.AccountCreateResetPassword);
            var resetLink = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).ResetPassword).Replace("[token]", token);
            emailTemplate = emailTemplate.Replace("[UserFullName]", userFullName)
                                         .Replace("[CreateNewPasswordLink]", resetLink)
                                         .Replace("[PasswordResetLink]", resetLink);
            return await _emailService.SendEmail(email, subject, emailTemplate);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            return false;
        }
    }

    public async Task<ApiResult<CustomerDto>> GetByCustomer(Guid id)
    {
        try
        {
            var entity = await _customerRepository.FirstOrDefaultAsync(t => t.Id == id , include: entity => entity.Include(e => e.Addresses.Where(a => !a.IsDeleted).OrderByDescending(e => e.CreatedAt)).Include(e => e.CustomerStatus).Include(e => e.Country), ignoreGlobalFilter: true);
            if (entity == null)
            {
                return ApiResult<CustomerDto>.NotFound("customer");
            }
            var mappedEntity = _mapper.Map<CustomerDto>(entity);

            return ApiResult<CustomerDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
}
