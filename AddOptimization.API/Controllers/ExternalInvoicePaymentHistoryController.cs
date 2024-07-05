using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using AddOptimization.Utilities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class ExternalInvoicePaymentHistoryController : CustomApiControllerBase
    {
        private readonly IExternalInvoicePaymentHistoryService _invoicePaymentService;
        public ExternalInvoicePaymentHistoryController(ILogger<ExternalInvoicePaymentHistoryController> logger, IExternalInvoicePaymentHistoryService invoicePaymentService) : base(logger)
        {
            _invoicePaymentService = invoicePaymentService;
        }
       

        [HttpPost]
        public async Task<IActionResult> Create(ExternalInvoiceAmountDto model)
        {
            try
            {
                var retVal = await _invoicePaymentService.Create(model);
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
                var retVal = await _invoicePaymentService.Search(filter);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
        [HttpGet("get-external-payment-details/{id}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            try
            {
                var retVal = await _invoicePaymentService.GetPaymentById(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

    }
}
