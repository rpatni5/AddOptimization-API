using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class QuoteStatusController : CustomApiControllerBase
    {
        private readonly IQuoteStatusService _quoteStatusService;
        public QuoteStatusController(ILogger<QuoteStatusController> logger, IQuoteStatusService quoteStatusService) : base(logger)
        {
            _quoteStatusService = quoteStatusService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search()
        {
            try
            {
                var result = await _quoteStatusService.Search();
                return HandleResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
