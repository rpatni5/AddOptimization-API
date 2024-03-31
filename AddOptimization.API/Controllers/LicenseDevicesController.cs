using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Dto;

namespace AddOptimization.API.Controllers;

public class LicenseDevicesController : CustomApiControllerBase
{

    private readonly ILicenseDeviceService _licensesDeviceService;
    public LicenseDevicesController(ILogger<LicenseController> logger, ILicenseDeviceService licensesDeviceService) : base(logger)
    {
        _licensesDeviceService = licensesDeviceService;
    }

    [Authorize]
    [HttpGet("licensedevice/{licenseId}")]
    public async Task<IActionResult> GetByLicenseId(Guid licenseId)
    {
        try
        {
            var retVal = await _licensesDeviceService.GetByLicenseId(licenseId);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [AllowAnonymous]
    [HttpPost("activate")]
    public async Task<IActionResult> ActivateLicenseByLicenseId(LicenseDeviceManagementDto request)
    {
        try
        {
            var retVal = await _licensesDeviceService.ActivateLicense(request);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [AllowAnonymous]
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateLicenseByLicenseId(LicenseDeviceManagementDto request)
    {
        try
        {
            var retVal = await _licensesDeviceService.ValidateLicense(request);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    [HasPermission(ScreenKeys.Licenses, GlobalFields.Delete)]
    public async Task<IActionResult> Delete(Guid id )
    {
        try
        {
            var retVal = await _licensesDeviceService.Delete(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
  
}