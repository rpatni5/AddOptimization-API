using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Models;
using AddOptimization.Services.Services;

namespace AddOptimization.API.Controllers;
[Authorize]
public class CustomersController : CustomApiControllerBase
{

    private readonly ICustomerService _customersService;
    public CustomersController(ILogger<CustomersController> logger, ICustomerService customersService) : base(logger)
    {
        _customersService = customersService;
    }

    [HttpPost("summary")]
    public async Task<IActionResult> GetSummary([FromBody] PageQueryFiterBase filter)
    {
        try
        {
            var retVal =await _customersService.GetSummary(filter);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("search")]
    public async Task<IActionResult> Get([FromBody] PageQueryFiterBase filter)
    {
        try
        {
            var retVal = await _customersService.Search(filter);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, bool includeOrderStats)
    {
        try
        {
            var retVal = await _customersService.Get(id, includeOrderStats);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }


    [HttpPost]
    [HasPermission(ScreenKeys.Customers, GlobalFields.Create)]
    public async Task<IActionResult> Create(CustomerCreateDto model)
    {
        try
        {
            var retVal = await _customersService.Create(model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPut("{id}")]
    [HasPermission(ScreenKeys.Customers, GlobalFields.Update)]
    public async Task<IActionResult> Update(Guid id, CustomerCreateDto model)
    {
        try
        {
            var retVal = await _customersService.Update(id, model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("send-email/{id}")]
    [HasPermission(ScreenKeys.Customers, GlobalFields.Update)]
    public async Task<IActionResult> SendEmail(Guid id)
    {
        try
        {
            var retVal = await _customersService.SendCustomerCreatedAndResetPasswordEmail(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpDelete("{id}")]
    [HasPermission(ScreenKeys.Customers, GlobalFields.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var retVal = await _customersService.Delete(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("get-customer/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCustomerById(Guid id)
    {
        try
        {
            var retVal = await _customersService.GetCustomerById(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllCustomers()
    {
        try
        {
            var retVal = await _customersService.GetAllCustomers();
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}