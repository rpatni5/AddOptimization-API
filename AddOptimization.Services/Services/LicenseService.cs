using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Services.Constants;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Enums;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Models;
using System.Globalization;
using AddOptimization.Contracts.Constants; 
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using AddOptimization.Utilities.Constants;
using Microsoft.Extensions.Configuration;

namespace AddOptimization.Services.Services;
public class LicenseService : ILicenseService
{
    private readonly IGenericRepository<License> _licenseRepository;
    private readonly IGenericRepository<Customer> _customerRepository;
    private readonly ILogger<LicenseService> _logger;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;
    public LicenseService(IGenericRepository<License> licenseRepository, ILogger<LicenseService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor,
        IGenericRepository<Customer> customerRepository,IConfiguration configuration, IUnitOfWork unitOfWork,IPermissionService permissionService)
    {
        _licenseRepository = licenseRepository;
        _logger = logger;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _customerRepository = customerRepository;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
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
    public async Task<ApiResult<LicenseDetailsDto>> Create(LicenseCreateDto model)
    {
        try
        {
            var entity = _mapper.Map<License>(model);
            entity.LicenseKey = "ASU76-NDHE7-MJDNF-YHT65-876BA"; //GenerateLicenseKey(); 
            _httpContextAccessor.HttpContext.GetCurrentUserFullName();
            entity.ExpirationDate = CalculateExpirationDateTime();
            //entity.Id = Guid.NewGuid();
            await _licenseRepository.InsertAsync(entity);
            return await Get(entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    private DateTime CalculateExpirationDateTime()
    {
        throw new NotImplementedException();
    }

    private string GenerateLicenseKey()
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResult<LicenseDetailsDto>> Update(Guid id, LicenseUpdateDto model)
    {
        try
        {
            var entity = await _licenseRepository.FirstOrDefaultAsync(o => o.Id == id, disableTracking: false,ignoreGlobalFilter:true);
            if (entity == null)
            {
                return ApiResult<LicenseDetailsDto>.NotFound("License");
            }
            if(model.ExpireLicense)
            {
                entity.ExpirationDate = DateTime.UtcNow;
            }
            if(entity.LicenseDuration != (int)model.LicenseDuration)
            {
                //Calculate New License ExpirationTime
            }
            entity.NoOfDevices = model.NoOfDevices;
            //_mapper.Map(model, entity);
            await _licenseRepository.UpdateAsync(entity);
            return await Get(id);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
}
