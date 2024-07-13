using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    [Authorize]

    public class InvoiceCreditNoteController : CustomApiControllerBase
    {
        private readonly IInvoiceCreditNoteService _invoiceCreditNoteService;
        public InvoiceCreditNoteController(ILogger<InvoiceCreditNoteController> logger, IInvoiceCreditNoteService invoicePaymentHistoryService) : base(logger)
        {
            _invoiceCreditNoteService = invoicePaymentHistoryService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(InvoiceCreditPaymentDto model)
        {
            try
            {
                var retVal = await _invoiceCreditNoteService.Create(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("get-credit-note-details/{id}")]
        public async Task<IActionResult> GetCreditInfoById(int id)
        {
            try
            {
                var retVal = await _invoiceCreditNoteService.GetCreditInfoById(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
