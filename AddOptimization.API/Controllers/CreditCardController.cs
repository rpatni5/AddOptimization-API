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
public class CreditCardController : CustomApiControllerBase
{

    private readonly ICreditCardService _creditCardService;
    public CreditCardController(ILogger<CreditCardController> logger, ICreditCardService creditCardService) : base(logger)
    {
        _creditCardService = creditCardService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(TemplateEntryDto model)
    {
        try
        {
            var retVal = await _creditCardService.SaveCreditCardDetails(model);
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
            var result = await _creditCardService.Search();
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("get-by-cardId/{id}")]
    public async Task<IActionResult> GetCardDetailsById(Guid id)
    {
        try
        {
            var retVal = await _creditCardService.GetCardDetailsById(id);
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
            var retVal = await _creditCardService.Update(id, model);
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
            var retVal = await _creditCardService.Delete(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

}