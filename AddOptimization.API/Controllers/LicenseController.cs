using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using AddOptimization.Utilities.Models;

namespace AddOptimization.API.Controllers;
[Authorize]
public class LicenseController : CustomApiControllerBase
{

    private readonly ILicenseService _licensesService;
    public LicenseController(ILogger<LicenseController> logger, ILicenseService licensesService) : base(logger)
    {
        _licensesService = licensesService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Get([FromBody] PageQueryFiterBase filters)
    {
        try
        {
            var retVal = await _licensesService.Search(filters);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        try
        {
            var retVal = await _licensesService.Get(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id,[FromBody]LicenseUpdateDto model)
    {
        try
        {
            var retVal = await _licensesService.Update(id,model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost]
    [HasPermission(ScreenKeys.Licenses,GlobalFields.Create)]
    public async Task<IActionResult> Create([FromBody] LicenseCreateDto model)
    {
        try
        {
            var retVal = await _licensesService.Create(model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}