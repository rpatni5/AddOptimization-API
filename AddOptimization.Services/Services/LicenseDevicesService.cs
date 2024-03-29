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

namespace AddOptimization.Services.Services;
public class LicenseDeviceService : ILicenseDeviceService
{
    private readonly IGenericRepository<LicenseDevice> _licenseDeviceRepository;
    private readonly IGenericRepository<Customer> _customerRepository;
    private readonly ILogger<LicenseService> _logger;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;
    public LicenseDeviceService(IGenericRepository<LicenseDevice> licenseDeviceRepository, ILogger<LicenseService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor,
        IGenericRepository<Customer> customerRepository, IConfiguration configuration, IUnitOfWork unitOfWork, IPermissionService permissionService)
    {
        _licenseDeviceRepository = licenseDeviceRepository;
        _logger = logger;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _customerRepository = customerRepository;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
    }

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
