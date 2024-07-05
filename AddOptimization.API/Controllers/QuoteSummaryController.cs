using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class QuoteSummaryController : CustomApiControllerBase
    {
        private readonly IQuoteSummaryService _quoteSummaryService;
        public QuoteSummaryController(ILogger<QuoteSummaryController> logger, IQuoteSummaryService quoteSummaryService) : base(logger)
        {
            _quoteSummaryService = quoteSummaryService;
        }

       
    }
}
