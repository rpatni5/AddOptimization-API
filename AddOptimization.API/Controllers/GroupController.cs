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
public class GroupController : CustomApiControllerBase
{

    private readonly IGroupService _groupService;
    public GroupController(ILogger<GroupController> logger, IGroupService groupService) : base(logger)
    {
        _groupService = groupService;
    }


    [HttpPost]
    public async Task<IActionResult> Create(CombineGroupModelDto model)
    {
        try
        {
            var retVal = await _groupService.Create(model);
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
            var result = await _groupService.GetAllGroups();
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }


    [HttpGet("get-by-groupId/{id}")]
    public async Task<IActionResult> GetGroupMembersByGroupId(Guid id)
    {
        try
        {
            var retVal = await _groupService.GetGroupAndMembersByGroupId(id);
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
            var retVal = await _groupService.Delete(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

}