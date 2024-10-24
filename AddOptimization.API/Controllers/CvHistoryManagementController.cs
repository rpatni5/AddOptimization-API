using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Services.Services;
using AddOptimization.Utilities.Models;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Services;

namespace AddOptimization.API.Controllers;
[Authorize]
public class CvHistoryManagementController : CustomApiControllerBase
{

    private readonly ICvManagementHistoryService _cvHistoryManagementService;
    public CvHistoryManagementController(ILogger<CvHistoryManagementController> logger, ICvManagementHistoryService cvHistoryManagementService) : base(logger)
    {
        _cvHistoryManagementService = cvHistoryManagementService;
    }

    [HttpGet("get-cv-history/{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            var retVal = await _cvHistoryManagementService.GetCvHistory(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }


    [HttpGet("get-cv-history-details/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var retVal = await _cvHistoryManagementService.GetHistoryDetailsById(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

}