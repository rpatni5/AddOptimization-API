﻿using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class InvoicePaymentModeController : CustomApiControllerBase
    {
        private readonly IInvoicingPaymentModeService _invoicePaymentModeService;
        public InvoicePaymentModeController(ILogger<InvoicePaymentModeController> logger, IInvoicingPaymentModeService invoicePaymentModeService) : base(logger)
        {
            _invoicePaymentModeService = invoicePaymentModeService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search()
        {
            try
            {
                var result = await _invoicePaymentModeService.Search();
                return HandleResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

       
    }
}