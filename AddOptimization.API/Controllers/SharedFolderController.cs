using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Services.Services;
using AddOptimization.Utilities.Models;
using Stripe;

namespace AddOptimization.API.Controllers;
[Authorize]
public class SharedFolderController : CustomApiControllerBase
{

    private readonly ISharedFolderService _sharedFolderService;
    public SharedFolderController(ILogger<SharedFolderController> logger, ISharedFolderService sharedFolderService) : base(logger)
    {
        _sharedFolderService = sharedFolderService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(SharedFolderRequestDto model)
    {
        try
        {
            var retVal = await _sharedFolderService.Create(model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("get-by-folderId/{id}")]
    public async Task<IActionResult> GetSharedFolderDataBySharedId(Guid id)
    {
        try
        {
            var retVal = await _sharedFolderService.GetSharedFolderDataBySharedId(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var retVal = await _sharedFolderService.Delete(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] FolderPermissionLevelDto model)
    {
        try
        {
            var retVal = await _sharedFolderService.Update(id, model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }


}