using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Services.Services;
using AddOptimization.Utilities.Models;

namespace AddOptimization.API.Controllers;
[Authorize]
public class SharedEntryController : CustomApiControllerBase
{

    private readonly ISharedEntryService _sharedEntryService;
    public SharedEntryController(ILogger<SharedEntryController> logger, ISharedEntryService sharedEntryService) : base(logger)
    {
        _sharedEntryService = sharedEntryService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(SharedEntryRequestDto model)
    {
        try
        {
            var retVal = await _sharedEntryService.Create(model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("get-by-entryId/{id}")]
    public async Task<IActionResult> GetSharedDataBySharedId(Guid id)
    {
        try
        {
            var retVal = await _sharedEntryService.GetSharedDataBySharedId(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

}