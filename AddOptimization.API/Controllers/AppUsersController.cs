using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Dto;
using Microsoft.AspNetCore.Authorization;
using AddOptimization.Contracts.Constants;
using AddOptimization.Utilities.Models;

namespace AddOptimization.API.Controllers;
[Authorize]
public class AppUsersController : CustomApiControllerBase
{
    private readonly IApplicationUserService _applicationUsersService;

    public AppUsersController(ILogger<AppUsersController> logger, IApplicationUserService usersService) : base(logger)
    {
        _applicationUsersService = usersService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody]UserCreateDto model)
    {
        try
        {
            var retVal = await _applicationUsersService.Create(model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("search")]
    public async Task<IActionResult> Get()
    {
        try
        {
            var retVal = await _applicationUsersService.Search();
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("account-admins")]
    public async Task<IActionResult> GetAccountAdmins()
    {
        try
        {
            var retVal = await _applicationUsersService.GetAccountAdmins();
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("searchsummary")]
    public async Task<IActionResult> SearchSummary(PageQueryFiterBase filters)
    {
        try
        {
            var retVal = await _applicationUsersService.SearchSummary(filters);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPut("{userId}")]
    [HasPermission(ScreenKeys.Users, GlobalFields.Update)]
    public async Task<IActionResult> UpdateSecurityData(int userId,[FromBody] UserUpdateDto model)
    {
        try
        {
            var retVal = await _applicationUsersService.Update(userId,model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }


    [HttpPut("toogleStatus/{id}")]
    [HasPermission(ScreenKeys.Users, GlobalFields.Update)]
    public async Task<IActionResult> ToogleActivationStatus(int id)
    {
        try
        {
            var retVal = await _applicationUsersService.ToogleActivationStatus(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
    [HttpPut("toogleEmailEnabled/{id}")]
    public async Task<IActionResult> ToggleEmailsEnabled(int id)
    {
        try
        {
            var retVal = await _applicationUsersService.ToggleEmailsEnabled(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

}
