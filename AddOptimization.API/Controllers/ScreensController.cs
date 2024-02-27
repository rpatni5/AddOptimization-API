using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;

namespace ShopMetrics.API.Controllers;

[Authorize]
public class ScreensController : CustomApiControllerBase
{
    private readonly IScreenService _screenService;

    public ScreensController(ILogger<ScreensController> logger, IScreenService screenService) : base(logger)
    {
        _screenService = screenService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search()
    {
        try
        {
            var result = await _screenService.Search();
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ScreenCreateDto model)
    {
        try
        {
            var result = await _screenService.Create(model);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id,[FromBody] ScreenCreateDto model)
    {
        try
        {
            var result = await _screenService.Update(id,model);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
