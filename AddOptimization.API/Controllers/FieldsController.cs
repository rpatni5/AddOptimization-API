using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Models;

namespace AddOptimization.API.Controllers;

[Authorize]
public class FieldsController : CustomApiControllerBase
{
    private readonly IFieldService _fieldService;

    public FieldsController(ILogger<FieldsController> logger, IFieldService fieldService) : base(logger)
    {
        _fieldService = fieldService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody]PageQueryFiterBase filters)
    {
        try
        {
            var result = await _fieldService.Search(filters);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FieldCreateDto model)
    {
        try
        {
            var result = await _fieldService.Create(model);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
