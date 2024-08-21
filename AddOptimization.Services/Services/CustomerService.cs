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
using Microsoft.Extensions.Configuration;
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
            }, e => includeDeleted || (e.CustomerStatus != null && e.CustomerStatus.Name != CustomerStatuses.Inactive), ignoreGlobalFilter: true);
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
            var superAdminRole = _currentUserRoles.Where(c => c.Contains("Super Admin", StringComparison.InvariantCultureIgnoreCase) || c.Contains("Account Admin", StringComparison.InvariantCultureIgnoreCase)).ToList();
            var entities = await _customerRepository.QueryAsync(include: entities => entities
            .Include(e => e.CustomerStatus).Include(e => e.Licenses).Include(e => e.BillingAddress).Include(e => e.Country).Include(e => e.PartnerCountry),orderBy: x => x.OrderBy(x => x.Organizations), ignoreGlobalFilter: superAdminRole.Count != 0);

            entities = ApplySorting(entities, filter?.Sorted?.FirstOrDefault());
            entities = ApplyFilters(entities, filter);
            var pagedResult = PageHelper<Customer, CustomerDto>.ApplyPaging(entities, filter, entities => entities.Select(e => new CustomerDto
            {
                Id = e.Id,
                Company = e.Organizations,
                Notes = e.Notes,
                Phone = e.Phone,
                CustomerStatusId = e.CustomerStatusId,
                Licenses = _mapper.Map<List<LicenseDetailsDto>>(e.Licenses),
                CountryCodeId = e.CountryCodeId.ToString(),
                CustomerStatusName = e.CustomerStatus.Name,
                BillingAddressString = e.BillingAddress == null ? null : $"{e.BillingAddress.Address1},{e.BillingAddress.Zip},{e.BillingAddress.City}",
                ManagerName = e.ManagerName,
                ManagerEmail = e.ManagerEmail,
                ManagerPhone = e.ManagerPhone,
                VAT = e.VAT,
                PaymentClearanceDays = e.PaymentClearanceDays,
                CountryId = e.CountryId,
                IsApprovalRequired = e.IsApprovalRequired,
                PartnerName = e.PartnerName,
                PartnerBankName = e.PartnerBankName,
                PartnerBankAccountName = e.PartnerBankAccountName,
                PartnerBankAccountNumber = e.PartnerBankAccountNumber,
                PartnerBankAddress = e.PartnerBankAddress,
                PartnerCountryId = e.PartnerCountryId,
                PartnerAddress = e.PartnerAddress,
                PartnerAddress2 = e.PartnerAddress2,
                PartnerVATNumber = e.PartnerVATNumber,
                PartnerVAT = e.PartnerVAT,
                VATNumber = e.VATNumber,
                PartnerDescriptions = e.PartnerDescriptions,
                State = e.State,
                PartnerState = e.PartnerState,
                Address = e.Address,
                Address2 = e.Address2,
                City = e.City,
                ZipCode = e.ZipCode,
                PartnerCity = e.PartnerCity,
                PartnerZipCode = e.PartnerZipCode,
                CountryNames = e.Country.CountryName,
                PartnerCompany = e.PartnerCompany,
                PartnerCountryNames = e.PartnerCountry.CountryName,
                PartnerEmail = e.PartnerEmail,
                PartnerPhone = e.PartnerPhone,
                AccountContactName = e.AccountContactName,
                AccountContactEmail = e.AccountContactEmail,
                Name = e.Name,
                Email = e.Email,
                AdministrationContactName = e.AdministrationContactName,
                AdministrationContactEmail = e.AdministrationContactEmail,
                TechnicalContactEmail = e.TechnicalContactEmail,
                TechnicalContactName = e.TechnicalContactName,
                IsAccountSAM = e.IsAccountSAM,
                IsAdministrationSAM = e.IsAdministrationSAM,
                IsTechnicalSAM = e.IsTechnicalSAM,

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
            var isExists = await _customerRepository.IsExist(t => t.ManagerEmail.ToLower() == model.ManagerEmail.ToLower(), ignoreGlobalFilter: true);
            var isExistInAppUser = await _applicationUserRepository.IsExist(t => t.Email.ToLower() == model.ManagerEmail.ToLower(), ignoreGlobalFilter: true);

            if (isExists || isExistInAppUser)
            {
                var errorMessage = isExistInAppUser ? "User already exists with some other role in the system." : "Customer already exists with same email.";
                return ApiResult<CustomerDto>.Failure(ValidationCodes.EmailUserNameAlreadyExists, errorMessage);
            }
            var entity = _mapper.Map<Customer>(model);
            entity = await _customerRepository.InsertAsync(entity);
            var appUserEntity = new ApplicationUser
            {
                Email = model.ManagerEmail,
                FullName = model.ManagerName,
                IsEmailsEnabled = true,
                IsActive = true,
                IsLocked = false,
                UserName = model.ManagerEmail
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

    public async Task<ApiResult<bool>> SendCustomerCreatedAndResetPasswordEmail(Guid id)
    {
        var entity = await _customerRepository.FirstOrDefaultAsync(t => t.Id == id, ignoreGlobalFilter: true);
        if (entity == null)
        {
            return ApiResult<bool>.NotFound("Send email to customer.");
        }
        var appUserEntityValue = (await _applicationUserRepository.QueryAsync(c => c.Email == entity.ManagerEmail && c.IsActive)).FirstOrDefault();
        string token = GenerateToken();
        var resetToken = new PasswordResetToken
        {
            Token = token,
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            CreatedByUserId = appUserEntityValue.Id,
        };
        resetToken = await _passwordResetTokenRepository.InsertAsync(resetToken);
        var isEmailSent = await SendCustomerCreatedAndResetPasswordEmail(token, appUserEntityValue.FullName, appUserEntityValue.Email);
        if (!isEmailSent)
        {
            await _passwordResetTokenRepository.DeleteAsync(resetToken);
            return ApiResult<bool>.Failure(ValidationCodes.IssueSendingEmail);
        }
        return ApiResult<bool>.Success(true);
    }

    public async Task<ApiResult<CustomerDto>> Update(Guid id, CustomerCreateDto model)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var isExists = await _customerRepository.IsExist(t => t.Id != id && t.ManagerEmail.ToLower() == model.ManagerEmail.ToLower(), ignoreGlobalFilter: true);

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
            if (entity.ManagerEmail != model.ManagerEmail || entity.ManagerName != model.ManagerName)
            {
                var appUserEntityValue = (await _applicationUserRepository.QueryAsync(c => c.Email == entity.ManagerEmail && c.IsActive)).FirstOrDefault();
                appUserEntityValue.FullName = model.Name;
                appUserEntityValue.Email = model.ManagerEmail;
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

    private IQueryable<Customer> ApplyFilters(IQueryable<Customer> entities, PageQueryFiterBase filter)
    {

        filter.GetValue<string>("Company", (v) =>
        {
            entities = entities.Where(e => e.Organizations != null && e.Organizations.ToLower().Contains(v.ToLower()));
        });
        filter.GetValue<string>("ManagerName", (v) =>
        {
            entities = entities.Where(e => e.ManagerName != null && e.ManagerName.ToLower().Contains(v.ToLower()));
        });

        filter.GetValue<string>("CountryNames", (v) =>
        {
            entities = entities.Where(e => e.Country.CountryName != null && e.Country.CountryName.ToLower().Contains(v.ToLower()));
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
                orders = orders.OrderBy(o => o.Organizations);
                return orders;
            }
            var columnName = sort.Name.ToUpper();
            if (sort.Direction == SortDirection.ascending.ToString())
            {

                if (columnName.ToUpper() == nameof(CustomerDto.Company).ToUpper())
                {
                    orders = orders.OrderBy(o => o.Organizations);
                }
                if (columnName.ToUpper() == nameof(CustomerDto.ManagerName).ToUpper())
                {
                    orders = orders.OrderBy(o => o.ManagerName);
                }
                if (columnName.ToUpper() == nameof(CustomerDto.CountryNames).ToUpper())
                {
                    orders = orders.OrderBy(o => o.Country.CountryName);
                }
                if (columnName.ToUpper() == nameof(CustomerDto.CustomerStatus).ToUpper())
                {
                    orders = orders.OrderBy(o => o.CustomerStatus);
                }

            }
            else
            {
                if (columnName.ToUpper() == nameof(CustomerDto.Company).ToUpper())
                {
                    orders = orders.OrderBy(o => o.Organizations);
                }
                if (columnName.ToUpper() == nameof(CustomerDto.ManagerName).ToUpper())
                {
                    orders = orders.OrderBy(o => o.ManagerName);
                }
                if (columnName.ToUpper() == nameof(CustomerDto.CountryNames).ToUpper())
                {
                    orders = orders.OrderBy(o => o.Country.CountryName);
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
            var subject = "Create your account password";
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

    public async Task<ApiResult<CustomerDto>> GetCustomerById(Guid id)
    {
        try
        {
            var entity = await _customerRepository.FirstOrDefaultAsync(t => t.Id == id, include: entity => entity.Include(e => e.Addresses.Where(a => !a.IsDeleted).OrderByDescending(e => e.CreatedAt)).Include(e => e.CustomerStatus).Include(e => e.Country).Include(e => e.PartnerCountry), ignoreGlobalFilter: true);
            if (entity == null)
            {
                return ApiResult<CustomerDto>.NotFound("Customer");
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

    public async Task<ApiResult<List<CustomerDto>>> GetAllCustomers()
    {
        try
        {
            var activeId = (await _customerStatusRepository.FirstOrDefaultAsync(e => e.Name == CustomerStatuses.Active)).Id;

            var entities = await _customerRepository.QueryAsync(
                e => e.CustomerStatusId == activeId,
                include: entities => entities
                    .Include(e => e.CreatedByUser)
                    .Include(e => e.UpdatedByUser)
                    .Include(e => e.Country),
                orderBy: x => x.OrderBy(x => x.Id)
            );

            var mappedEntities = entities.Select(e => new CustomerDto
            {
                Id = e.Id,
                Company = e.Organizations,
                Notes = e.Notes,
                Phone = e.Phone,
                CustomerStatusId = e.CustomerStatusId,
                Licenses = _mapper.Map<List<LicenseDetailsDto>>(e.Licenses),
                CountryCodeId = e.CountryCodeId.ToString(),
                CustomerStatusName = e.CustomerStatus.Name,
                BillingAddressString = e.BillingAddress == null ? null : $"{e.BillingAddress.Address1},{e.BillingAddress.Zip},{e.BillingAddress.City}",
                ManagerName = e.ManagerName,
                ManagerEmail = e.ManagerEmail,
                ManagerPhone = e.ManagerPhone,
                VAT = e.VAT,
                PartnerVAT = e.PartnerVAT,
                PaymentClearanceDays = e.PaymentClearanceDays,
                CountryId = e.CountryId,
                IsApprovalRequired = e.IsApprovalRequired,
                PartnerName = e.PartnerName,
                PartnerBankName = e.PartnerBankName,
                PartnerBankAccountName = e.PartnerBankAccountName,
                PartnerBankAccountNumber = e.PartnerBankAccountNumber,
                PartnerBankAddress = e.PartnerBankAddress,
                PartnerCountryId = e.PartnerCountryId,
                PartnerAddress = e.PartnerAddress,
                PartnerAddress2 = e.PartnerAddress2,
                PartnerVATNumber = e.PartnerVATNumber,
                VATNumber = e.VATNumber,
                PartnerDescriptions = e.PartnerDescriptions,
                State = e.State,
                PartnerState = e.PartnerState,
                Address = e.Address,
                Address2 = e.Address2,
                City = e.City,
                ZipCode = e.ZipCode,
                PartnerCity = e.PartnerCity,
                PartnerZipCode = e.PartnerZipCode,
                CountryNames = e.Country.CountryName,
                PartnerCompany = e.PartnerCompany,
                PartnerCountryNames = e.PartnerCountry.CountryName,
                PartnerEmail = e.PartnerEmail,
                PartnerPhone = e.PartnerPhone,
                AccountContactName = e.AccountContactName,
                AccountContactEmail = e.AccountContactEmail,
                Name = e.Name,
                Email = e.Email,
                AdministrationContactName = e.AdministrationContactName,
                AdministrationContactEmail = e.AdministrationContactEmail,
                TechnicalContactEmail = e.TechnicalContactEmail,
                TechnicalContactName = e.TechnicalContactName,
                IsAccountSAM = e.IsAccountSAM,
                IsAdministrationSAM = e.IsAdministrationSAM,
                IsTechnicalSAM = e.IsTechnicalSAM,

            }).ToList();

            return ApiResult<List<CustomerDto>>.Success(mappedEntities);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
}
