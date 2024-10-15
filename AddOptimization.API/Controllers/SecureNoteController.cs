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
public class SecureNoteController : CustomApiControllerBase
{

    private readonly ISecureNoteService _secureNoteService;
    public SecureNoteController(ILogger<SecureNoteController> logger, ISecureNoteService secureNoteService) : base(logger)
    {
        _secureNoteService = secureNoteService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(TemplateEntryDto model)
    {
        try
        {
            var retVal = await _secureNoteService.SaveSecureNote(model);
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
            var result = await _secureNoteService.Search();
            return HandleResponse(result);
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
            var retVal = await _secureNoteService.Delete(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
    [HttpGet("get-by-noteId/{id}")]
    public async Task<IActionResult> GetSecureNoteById(Guid id)
    {
        try
        {
            var retVal = await _secureNoteService.GetSecureNoteById(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TemplateEntryDto model)
    {
        try
        {
            var retVal = await _secureNoteService.Update(id, model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}