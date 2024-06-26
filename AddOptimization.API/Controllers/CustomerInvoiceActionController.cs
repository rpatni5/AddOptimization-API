using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Models;
using AddOptimization.Contracts.Dto;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using AddOptimization.Services.Services;
using AddOptimization.Utilities.Services;

namespace AddOptimization.API.Controllers
{
    [AllowAnonymous]
    public class CustomerInvoiceActionController : CustomApiControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly CustomDataProtectionService _customDataProtectionService;

        public CustomerInvoiceActionController(ILogger<CustomerInvoiceActionController> logger, IInvoiceService invoiceService, CustomDataProtectionService customDataProtectionService) : base(logger)
        {
            _invoiceService = invoiceService;
            _customDataProtectionService = customDataProtectionService;
        }

        [HttpGet("invoice-details/{id}")]
        public async Task<IActionResult> GetInvoiceDetails(string id)
        {
            try
            {
                var eventId = int.Parse(_customDataProtectionService.Decode(id));
                var retVal = await _invoiceService.FetchInvoiceDetails(eventId, false);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
        [HttpPost("decline-request")]
        public async Task<IActionResult> DeclineRequest(InvoiceActionRequestDto model)
        {
            try
            {
                var retVal = await _invoiceService.DeclineRequest(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
