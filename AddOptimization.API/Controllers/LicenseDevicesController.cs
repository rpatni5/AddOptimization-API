using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Services;

namespace AddOptimization.API.Controllers;
[Authorize]
public class LicenseDevicesController : CustomApiControllerBase
{

    private readonly ILicenseDeviceService _licensesDeviceService;
    public LicenseDevicesController(ILogger<LicenseController> logger, ILicenseDeviceService licensesDeviceService) : base(logger)
    {
        _licensesDeviceService = licensesDeviceService;
    }

    [HttpGet("license/{licenseId}")]
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