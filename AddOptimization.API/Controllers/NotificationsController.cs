using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers;

[Authorize]
public class NotificationsController : CustomApiControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(ILogger<NotificationsController> logger, INotificationService notificationService) : base(logger)
    {
        _notificationService = notificationService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] PageQueryFiterBase filters)
    {
        try
        {
            var result = await _notificationService.Search(filters);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPut("toggleRead/{id?}")]
    public async Task<IActionResult> ToggleRead(int? id)
    {
        try
        {
            var result = await _notificationService.ToggleRead(id);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPut("refreshReadCount/{id?}")]
    public async Task<IActionResult> RefreshReadCount(int? id)
    {
        try
        {

            await _notificationService.NotifyUser(id, "notification refreshed");

            return Ok();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

}

