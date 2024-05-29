using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;

namespace AddOptimization.API.Controllers;
[Authorize]
public class CompanyController : CustomApiControllerBase
{

    private readonly ICompanyService _companyService;
    public CompanyController(ILogger<CompanyController> logger, ICompanyService companyService) : base(logger)
    {
        _companyService = companyService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CompanyDto model)
    {
        try
        {
            var retVal = await _companyService.Create(model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("get-company-information")]
    public async Task<IActionResult> GetCompanyInformation()
    {
        try
        {
            var retVal = await _companyService.GetCompanyInformation();
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}