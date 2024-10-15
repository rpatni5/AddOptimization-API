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
public class PersonalInformationController : CustomApiControllerBase

{
    private readonly IPersonalInformationService _personalInformationService;
    public PersonalInformationController(ILogger<PersonalInformationController> logger, IPersonalInformationService personalInformationService) : base(logger)
    {
        _personalInformationService = personalInformationService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(TemplateEntryDto model)
    {
        try
        {
            var retVal = await _personalInformationService.SavePersonalInformationDetails(model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TemplateEntryDto model)
    {
        try
        {
            var retVal = await _personalInformationService.Update(id, model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("get-by-infoId/{id}")]
    public async Task<IActionResult> GetCardDetailsById(Guid id)
    {
        try
        {
            var retVal = await _personalInformationService.GetInfoDetailsById(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search()
    {
        try
        {
            var result = await _personalInformationService.Search();
            return HandleResponse(result);
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
            var retVal = await _personalInformationService.Delete(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

}
