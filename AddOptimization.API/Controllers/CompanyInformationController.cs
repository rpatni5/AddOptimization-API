using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers;
[Authorize]
public class CompanyInformationController : CustomApiControllerBase

{
    private readonly ICompanyInformationService _companyInformationService;
    public CompanyInformationController(ILogger<CompanyInformationController> logger, ICompanyInformationService companyInformationService) : base(logger)
    {
        _companyInformationService = companyInformationService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(TemplateEntryDto model)
    {
        try
        {
            var retVal = await _companyInformationService.SaveCompanyInformationDetails(model);
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
            var retVal = await _companyInformationService.Update(id, model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("get-by-companyId/{id}")]
    public async Task<IActionResult> GetCompanyDetailsById(Guid id)
    {
        try
        {
            var retVal = await _companyInformationService.GetCompanyDetailsById(id);
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
            var result = await _companyInformationService.Search();
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
            var retVal = await _companyInformationService.Delete(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

}

