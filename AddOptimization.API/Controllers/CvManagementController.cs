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
public class CvManagementController : CustomApiControllerBase
{

    private readonly ICvManagementService _cvManagementService;
    public CvManagementController(ILogger<CvManagementController> logger, ICvManagementService cvManagementService) : base(logger)
    {
        _cvManagementService = cvManagementService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromForm] CvEntryDto model)
    {
        try
        {
            var retVal = await _cvManagementService.Save(model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)

        {
            return HandleException(ex);
        }
    }

}