using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class PaymentStatusController : CustomApiControllerBase
    {
        private readonly IPaymentStatusService _paymentStatusService;
        public PaymentStatusController(ILogger<PaymentStatusController> logger, IPaymentStatusService paymentStatusService) : base(logger)
        {
            _paymentStatusService = paymentStatusService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search()
        {
            try
            {
                var result = await _paymentStatusService.Search();
                return HandleResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
