using AddOptimization.API.Common;
using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using AddOptimization.Utilities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class QuoteController : CustomApiControllerBase
    {
        private readonly IQuoteService _quoteService;
        public QuoteController(ILogger<QuoteController> logger, IQuoteService quoteService) : base(logger)
        {
            _quoteService = quoteService;
        }

        [HttpPost("search")]
        public async Task<IActionResult> Get([FromBody] PageQueryFiterBase filters)
        {
            try
            {
                var retVal = await _quoteService.Search(filters);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


        [HttpPost]
        public async Task<IActionResult> Create(QuoteRequestDto model)
        {
            try
            {
                var retVal = await _quoteService.Create(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, QuoteRequestDto model)
        {
            try
            {
                var retVal = await _quoteService.Update(id, model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("get-quote-details/{id}")]
        public async Task<IActionResult> FetchItemConfDetails(long id)
        {
            try
            {
                var retVal = await _quoteService.FetchQuoteDetails(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
