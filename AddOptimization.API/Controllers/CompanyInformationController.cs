using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;

namespace UsersManagment.API.Controllers;
[Authorize]
public class CompanyInformationController : CustomApiControllerBase
{

    private readonly ICompanyInformationService _companyInformationService;
    public CompanyInformationController(ILogger<CompanyInformationController> logger, ICompanyInformationService companyService) : base(logger)
    {
        _companyInformationService = companyService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CompanyInformationDto model)
    {
        try
        {
            var retVal = await _companyInformationService.Create(model);
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
            var retVal = await _companyInformationService.GetCompanyInformation();
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}