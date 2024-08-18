using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class InvoiceStatusController : CustomApiControllerBase
    {
        private readonly IInvoiceStatusService _invoiceStatusService;
        public InvoiceStatusController(ILogger<InvoiceStatusController> logger, IInvoiceStatusService invoiceStatusService) : base(logger)
        {
            _invoiceStatusService = invoiceStatusService;
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search()
        {
            try
            {
                var result = await _invoiceStatusService.Search();
                return HandleResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
