using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Enums;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using AddOptimization.Contracts.Constants;
using AddOptimization.Utilities.Interface;
using AddOptimization.Utilities.Services;

namespace AddOptimization.Services.Services;
public class LicenseService : ILicenseService
{
    private readonly IConfiguration _configuration;
    private readonly ITemplateService _templateService;
    private readonly IGenericRepository<License> _licenseRepository;
    private readonly IGenericRepository<Customer> _customerRepository;
    private readonly ILogger<LicenseService> _logger;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IPermissionService _permissionService;
    private readonly List<string> _currentUserRoles;
    public LicenseService(IGenericRepository<License> licenseRepository, ILogger<LicenseService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor,
        IGenericRepository<Customer> customerRepository, IConfiguration configuration, ITemplateService templateService, IEmailService emailService, IUnitOfWork unitOfWork, IPermissionService permissionService)
    {
        _licenseRepository = licenseRepository;
        _logger = logger;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _customerRepository = customerRepository;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
        _templateService = templateService;
        _configuration = configuration;
        _emailService = emailService;
        _currentUserRoles = httpContextAccessor.HttpContext.GetCurrentUserRoles();
    }

    public async Task<PagedApiResult<LicenseDetailsDto>> Search(PageQueryFiterBase filter)
    {
        try
        {
            var superAdminRole = _currentUserRoles.Where(c => c.Contains("Super Admin")).ToList();

            var entities = await _licenseRepository.QueryAsync(include: source => source.Include(o => o.LicenseDevices).Include(o => o.Customer).Include(e => e.CreatedByUser), ignoreGlobalFilter: superAdminRole.Count != 0);

            entities = ApplySorting(entities, filter?.Sorted?.FirstOrDefault());
            entities = ApplyFilters(entities, filter);
            var pagedResult = PageHelper<License, LicenseDetailsDto>.ApplyPaging(entities, filter, entities => entities.Select(e => new LicenseDetailsDto
            {
                Id = e.Id,
                NoOfDevices = e.NoOfDevices,
                UpdatedAt = e.UpdatedAt,
                CustomerId = e.CustomerId,
                CreatedAt = e.CreatedAt,
                LicenseKey = e.LicenseKey,
                ExpirationDate = e.ExpirationDate,
                CustomerEmail = e.Customer.Email,
                LicenseDuration = e.LicenseDuration,
                CustomerName = e.Customer.Name,
                CreatedBy = e.CreatedByUser.FullName,
                LicenseDevices = _mapper.Map<List<LicenseDeviceDto>>(e.LicenseDevices),
                ActivatedDevicesCount = e.LicenseDevices.Any() ? e.LicenseDevices.Count() : 0,
                PendingDevicesCount = e.NoOfDevices - (e.LicenseDevices.Any() ? e.LicenseDevices.Count() : 0),
            }).ToList());
            var retVal = pagedResult;
            return PagedApiResult<LicenseDetailsDto>.Success(retVal);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    public async Task<ApiResult<LicenseDetailsDto>> Get(Guid licenseId)
    {
        try
        {
            var entity = await _licenseRepository.FirstOrDefaultAsync(o => o.Id == licenseId, include: source => source.Include(o => o.Customer).Include(e => e.LicenseDevices), ignoreGlobalFilter: true);
            if (entity == null)
            {
                return ApiResult<LicenseDetailsDto>.NotFound("License");
            }
            var mappedEntity = _mapper.Map<LicenseDetailsDto>(entity);
            return ApiResult<LicenseDetailsDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<List<LicenseDetailsDto>>> GetByCustomerId(Guid customerId)
    {
        try
        {
            var entity = await _licenseRepository.QueryAsync(o => o.CustomerId == customerId, include: source => source.Include(o => o.Customer).Include(e => e.LicenseDevices), ignoreGlobalFilter: true);
            if (entity == null || !entity.Any())
            {
                return ApiResult<List<LicenseDetailsDto>>.NotFound("License");
            }
            var mappedEntity = _mapper.Map<List<LicenseDetailsDto>>(entity);
            return ApiResult<List<LicenseDetailsDto>>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    public async Task<ApiResult<LicenseDetailsDto>> Create(LicenseCreateDto model)
    {
        try
        {
            var entity = _mapper.Map<License>(model);
            var licenseKey = LicenseKeyHelper.GenerateLicenseKey(model.CustomerId.ToString(), TimeSpan.FromDays((int)model.LicenseDuration));
            var isValid = LicenseKeyHelper.ValidateLicenseKey(licenseKey);
            entity.LicenseKey = licenseKey.Key;
            entity.ExpirationDate = licenseKey.ExpirationDate;
            _httpContextAccessor.HttpContext.GetCurrentUserFullName();
            var licenseResult = await _licenseRepository.InsertAsync(entity);
            if (licenseResult != null)
            {
                await SendCustomerLicenseAddedEmail(entity.Customer.Name, entity.Customer.Email, licenseResult);
            }
            return await Get(entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<LicenseDetailsDto>> Update(Guid id, LicenseUpdateDto model)
    {
        try
        {
            var entity = await _licenseRepository.FirstOrDefaultAsync(o => o.Id == id, disableTracking: false, ignoreGlobalFilter: true);
            if (entity == null)
            {
                return ApiResult<LicenseDetailsDto>.NotFound("License");
            }
            if (model.ExpireLicense)
            {
                entity.ExpirationDate = DateTime.UtcNow;
            }
            if (entity.LicenseDuration != (int)model.LicenseDuration)
            {
                var licenseDuration = TimeSpan.FromDays((int)model.LicenseDuration);
                entity.ExpirationDate = entity.CreatedAt.Value.Add(licenseDuration);
            }
            entity.NoOfDevices = model.NoOfDevices;
            entity.CustomerId = model.CustomerId;
            entity.LicenseDuration = (int)model.LicenseDuration;
            await _licenseRepository.UpdateAsync(entity);
            return await Get(id);
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
            var entity = await _licenseRepository.FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null)
            {
                return ApiResult<bool>.NotFound("License");
            }

            await _licenseRepository.DeleteAsync(entity);
            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    private IQueryable<License> ApplyFilters(IQueryable<License> entities, PageQueryFiterBase filter)
    {

        filter.GetValue<string>("customerName", (v) =>
        {
            entities = entities.Where(e => e.Customer != null && e.Customer.Name.ToLower().Contains(v.ToLower()));
        });

        filter.GetValue<string>("customerId", (v) =>
        {
            entities = entities.Where(e => e.CustomerId.ToString() == v);
        });

        filter.GetValue<DateTime>("createdAt", (v) =>
        {
            entities = entities.Where(e => e.CreatedAt != null && e.CreatedAt < v);
        }, OperatorType.lessthan, true);
        filter.GetValue<DateTime>("createdAt", (v) =>
        {
            entities = entities.Where(e => e.CreatedAt != null && e.CreatedAt > v);
        }, OperatorType.greaterthan, true);
        filter.GetValue<DateTime>("expirationTime", (v) =>
        {
            entities = entities.Where(e => e.ExpirationDate != DateTime.MinValue ? e.ExpirationDate.Date == v.Date : e.ExpirationDate.Date == DateTime.MinValue);
        });
        filter.GetValue<string>("licenseKey", (v) =>
        {
            entities = entities.Where(e => e.LicenseKey != null && e.LicenseKey == v);
        });

        filter.GetValue<string>("licenseId", (v) =>
        {
            entities = entities.Where(e => e.Id.ToString() == v);
        });
        return entities;
    }
    private IQueryable<License> ApplySorting(IQueryable<License> orders, SortModel sort)
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
                if (columnName.ToUpper() == nameof(LicenseDetailsDto.NoOfDevices).ToUpper())
                {
                    orders = orders.OrderBy(o => o.NoOfDevices);
                }
                if (columnName.ToUpper() == nameof(LicenseDetailsDto.CustomerId).ToUpper())
                {
                    orders = orders.OrderBy(o => o.Customer.Id);
                }
                if (columnName.ToUpper() == nameof(LicenseDetailsDto.CreatedAt).ToUpper())
                {
                    orders = orders.OrderBy(o => o.CreatedAt);
                }
                if (columnName.ToUpper() == nameof(LicenseDetailsDto.ExpirationDate).ToUpper())
                {
                    orders = orders.OrderBy(e => e.ExpirationDate);
                }
                if (columnName.ToUpper() == nameof(LicenseDetailsDto.CustomerName).ToUpper())
                {
                    orders = orders.OrderBy(o => o.Customer.Name);
                }
                if (columnName.ToUpper() == nameof(LicenseDetailsDto.CustomerEmail).ToUpper())
                {
                    orders = orders.OrderBy(o => o.Customer.Email);
                }
                if (columnName.ToUpper() == nameof(LicenseDetailsDto.LicenseDuration).ToUpper())
                {
                    orders = orders.OrderBy(o => o.LicenseDuration);
                }
            }
            else
            {
                if (columnName.ToUpper() == nameof(LicenseDetailsDto.NoOfDevices).ToUpper())
                {
                    orders = orders.OrderByDescending(o => o.NoOfDevices);
                }
                if (columnName.ToUpper() == nameof(LicenseDetailsDto.CustomerId).ToUpper())
                {
                    orders = orders.OrderByDescending(o => o.Customer.Id);
                }
                if (columnName.ToUpper() == nameof(LicenseDetailsDto.CreatedAt).ToUpper())
                {
                    orders = orders.OrderByDescending(o => o.CreatedAt);
                }
                if (columnName.ToUpper() == nameof(LicenseDetailsDto.ExpirationDate).ToUpper())
                {
                    orders = orders.OrderByDescending(e => e.ExpirationDate);
                }
                if (columnName.ToUpper() == nameof(LicenseDetailsDto.CustomerName).ToUpper())
                {
                    orders = orders.OrderByDescending(o => o.Customer.Name);
                }
                if (columnName.ToUpper() == nameof(LicenseDetailsDto.CustomerEmail).ToUpper())
                {
                    orders = orders.OrderByDescending(o => o.Customer.Email);
                }
                if (columnName.ToUpper() == nameof(LicenseDetailsDto.LicenseDuration).ToUpper())
                {
                    orders = orders.OrderByDescending(o => o.LicenseDuration);
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

    private async Task<bool> SendCustomerLicenseAddedEmail(string email, string userFullName, License license)
    {
        try
        {
            var subject = "Add optimization new license details";
            var message = "A new license has been created for your account. Please find the details below.";
            var emailTemplate = _templateService.ReadTemplate(EmailTemplates.CreateLicense);
            emailTemplate = emailTemplate
                            .Replace("[CustomerName]", userFullName)
                            .Replace("[Message]", message)
                            .Replace("[LicenseKey]", license.LicenseKey)
                            .Replace("[NoOfDevices]", license.NoOfDevices.ToString())
                            .Replace("[ExpirationDate]", license.ExpirationDate.ToString());
            return await _emailService.SendEmail(email, subject, emailTemplate);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            return false;
        }
    }
}
