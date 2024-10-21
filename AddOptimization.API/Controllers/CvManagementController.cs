using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Services.Services;
using AddOptimization.Utilities.Models;
using AddOptimization.Utilities.Common;

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

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] PageQueryFiterBase filters)
    {
        try
        {
            var retVal = await _cvManagementService.Search(filters);
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
            var retVal = await _cvManagementService.Delete(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("get-cv-details/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var retVal = await _cvManagementService.GetById(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
   

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id,[FromForm] CvEntryDto model)
    {
        try
        {
            var retVal = await _cvManagementService.Update(id, model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

}