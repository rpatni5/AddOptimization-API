using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using AddOptimization.Utilities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class InvoicePaymentHistoryController : CustomApiControllerBase
    {
        private readonly IInvoicePaymentHistoryService _invoicePaymentHistoryService;
        public InvoicePaymentHistoryController(ILogger<InvoicePaymentHistoryController> logger, IInvoicePaymentHistoryService invoicePaymentHistoryService) : base(logger)
        {
            _invoicePaymentHistoryService = invoicePaymentHistoryService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] List<InvoicePaymentHistoryDto> model)
        {
            try
            {
                var retVal = await _invoicePaymentHistoryService.Create(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("search")]
        public async Task<IActionResult> Get([FromBody] PageQueryFiterBase filter)
        {
            try
            {
                var retVal = await _invoicePaymentHistoryService.Search(filter);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("get-payment-details/{id}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            try
            {
                var retVal = await _invoicePaymentHistoryService.GetPaymentById(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


    }
}
