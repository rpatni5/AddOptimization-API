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
public class EmployeeController : CustomApiControllerBase
{

    private readonly IEmployeeService _employeeService;
    public EmployeeController(ILogger<EmployeeController> logger, IEmployeeService employeeService) : base(logger)
    {
        _employeeService = employeeService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(EmployeeDto model)
    {
        try
        {
            var retVal = await _employeeService.Save(model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, EmployeeDto model)
    {
        try
        {
            var retVal = await _employeeService.Update(id , model);
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
            var retVal = await _employeeService.Search(filters);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("get-employee/{id}")]
    public async Task<IActionResult> GetEmployeeById(Guid id)
    {
        try
        {
            var retVal = await _employeeService.GetEmployeeById(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

}