using AutoMapper;
using Microsoft.Extensions.Logging;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Enums;

namespace AddOptimization.Services.Services;
public class LicenseDeviceService : ILicenseDeviceService
{
    #region Private Fields
    private readonly IGenericRepository<License> _licenseRepository;
    private readonly IGenericRepository<LicenseDevice> _licenseDeviceRepository;
    private readonly IGenericRepository<Customer> _customerRepository;
    private readonly ILogger<LicenseService> _logger;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;
    #endregion

    #region Constructor
    public LicenseDeviceService(IGenericRepository<License> licenseRepository, IGenericRepository<LicenseDevice> licenseDeviceRepository, ILogger<LicenseService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor,
        IGenericRepository<Customer> customerRepository, IConfiguration configuration, IUnitOfWork unitOfWork, IPermissionService permissionService)
    {
        _licenseRepository = licenseRepository;
        _licenseDeviceRepository = licenseDeviceRepository;
        _logger = logger;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _customerRepository = customerRepository;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
    }

    #endregion

    #region Public Methods
    public async Task<ApiResult<List<LicenseDeviceDto>>> GetByLicenseId(Guid licenseId)
    {
        try
        {
            var entity = await _licenseDeviceRepository.QueryAsync(include: entities => entities
            .Include(e => e.CreatedByUser), predicate: o => o.LicenseId == licenseId, ignoreGlobalFilter: true);

            if (entity == null)
            {
                return ApiResult<List<LicenseDeviceDto>>.NotFound("License Devices");
            }
            var mappedEntity = _mapper.Map<List<LicenseDeviceDto>>(entity);
            return ApiResult<List<LicenseDeviceDto>>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    public async Task<ApiResult<LicenseDeviceDto>> ActivateLicense(LicenseDeviceManagementDto request)
    {
        try
        {
            var license = (await _licenseRepository.QueryAsync(include: entities => entities
            .Include(
                e => e.CreatedByUser).
                Include(c => c.Customer).
                Include(c => c.Customer.CustomerStatus).
                Include(l => l.LicenseDevices), predicate: o => o.LicenseKey == request.LicenseKey && o.Customer.CustomerStatus.Name == nameof(CustomerStatusEnum.Active), ignoreGlobalFilter: true)).FirstOrDefault();
            if (license == null || license.LicenseDevices.Any(c => c.MachineName == request.MachineName))
            {
                var message = "Cannot activate license to this device.Please connect with the System Administrator.";
                return ApiResult<LicenseDeviceDto>.Failure(ValidationCodes.CannotActivateLicense, message);
            }
            var activeDevices = license.LicenseDevices.Any() ? license.LicenseDevices.Count() : 0;
            var remainingLicense = license.NoOfDevices - activeDevices;
            if (remainingLicense > 0)
            {
                if (license.Customer.CustomerStatus.Name == nameof(CustomerStatusEnum.Active) && license.ExpirationDate > DateTime.UtcNow)
                {
                    if (license.NoOfDevices > activeDevices)
                    {
                        var licenseDevice = _mapper.Map<LicenseDevice>(request);
                        licenseDevice.CustomerId = license.CustomerId;
                        licenseDevice.LicenseId = license.Id;
                        await _licenseDeviceRepository.InsertAsync(licenseDevice);
                        var mappedLicenseDevice = _mapper.Map<LicenseDeviceDto>(licenseDevice);
                        return ApiResult<LicenseDeviceDto>.Success(mappedLicenseDevice);
                    }
                }
            }
            return ApiResult<LicenseDeviceDto>.Failure(ValidationCodes.CannotActivateLicense, "Cannot activate license to this device.Please connect with the System Administrator.");
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<bool>> ValidateLicense(LicenseDeviceManagementDto request)
    {
        try
        {
            var license = (await _licenseRepository.QueryAsync(include: entities => entities
            .Include(
                e => e.CreatedByUser).
                Include(c => c.Customer).
                Include(c => c.Customer.CustomerStatus).
                Include(l => l.LicenseDevices), predicate: o => o.LicenseKey == request.LicenseKey && o.Customer.CustomerStatus.Name == nameof(CustomerStatusEnum.Active), ignoreGlobalFilter: true)).FirstOrDefault();
            if (license == null || (license.LicenseDevices != null && !license.LicenseDevices.Any(x => x.MachineName == request.MachineName)))
            {
                var message = "License invalid.Please connect with the System Administrator.";
                return ApiResult<bool>.Failure(ValidationCodes.InvalidLicense, message);
            }
            var activeDevices = license.LicenseDevices.Any() ? license.LicenseDevices.Count() : 0;
            var remainingLicense = license.NoOfDevices - activeDevices;
            if (remainingLicense >= 0)
            {
                if (license.Customer.CustomerStatus.Name == nameof(CustomerStatusEnum.Active) && license.ExpirationDate > DateTime.UtcNow)
                {
                    if (license.NoOfDevices >= activeDevices)
                    {
                        return ApiResult<bool>.Success(true);
                    }
                }
            }
            return ApiResult<bool>.Failure(ValidationCodes.InvalidLicense, "License invalid.Please connect with the System Administrator.");
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<bool>> RemoveLicense(LicenseDeviceManagementDto request)
    {
        try
        {
            var license = (await _licenseRepository.QueryAsync(include: entities => entities
            .Include(
                e => e.CreatedByUser).
                Include(c => c.Customer).
                Include(c => c.Customer.CustomerStatus).
                Include(l => l.LicenseDevices),
                predicate: o => o.LicenseKey == request.LicenseKey &&
                           o.Customer.CustomerStatus.Name == nameof(CustomerStatusEnum.Active), ignoreGlobalFilter: true)).FirstOrDefault();
            if (license == null)
            {
                var message = "Cannot remove license. Please connect with the System Administrator.";
                return ApiResult<bool>.Failure(ValidationCodes.CannotActivateLicense, message);
            }
            if (license != null && license.LicenseDevices.Any())
            {
                var licenseId = license.LicenseDevices.FirstOrDefault(c => c.MachineName == request.MachineName).Id;
                var isDeleted = await Delete(licenseId);
                return ApiResult<bool>.Success(true);
            }
            return ApiResult<bool>.Failure(ValidationCodes.CannotActivateLicense, "Cannot remove license to this device.Please connect with the System Administrator.");
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
            var entity = await _licenseDeviceRepository.FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null)
            {
                return ApiResult<bool>.NotFound("License Devices");
            }

            await _licenseDeviceRepository.DeleteAsync(entity);
            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    #endregion
}
