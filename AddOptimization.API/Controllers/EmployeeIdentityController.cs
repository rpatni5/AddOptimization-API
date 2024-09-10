using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers;
[Authorize]
public class EmployeeIdentityController : CustomApiControllerBase
{
    private readonly IEmployeeIdentityService _employeeIdentityService;

    public EmployeeIdentityController(ILogger<EmployeeIdentityController> logger, IEmployeeIdentityService employeeIdentityService) : base(logger)
    {
        _employeeIdentityService = employeeIdentityService;
    }
     
    [HttpPost("search")]
    public async Task<IActionResult> Search()
    {
        try
        {
            var result = await _employeeIdentityService.Search();
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}

