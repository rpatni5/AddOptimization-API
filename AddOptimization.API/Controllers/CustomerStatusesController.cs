using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers;
[Authorize]
public class CustomerStatusesController : CustomApiControllerBase
{
    private readonly ICustomerStatusService _customerStatusService;

    public CustomerStatusesController(ILogger<CustomerStatusesController> logger, ICustomerStatusService customerStatusService) : base(logger)
    {
        _customerStatusService = customerStatusService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search()
    {
        try
        {
            var result = await _customerStatusService.Search();
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}

