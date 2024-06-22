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
    public class InvoiceController : CustomApiControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        public InvoiceController(ILogger<InvoiceController> logger, IInvoiceService invoiceService) : base(logger)
        {
            _invoiceService = invoiceService;
        }


        [HttpPost]
        public async Task<IActionResult> Create(InvoiceRequestDto model)
        {
            try
            {
                var retVal = await _invoiceService.Create(model);
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
                var retVal = await _invoiceService.Search(filter);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("get-invoice-details/{id}")]
        public async Task<IActionResult> FetchItemConfDetails(int id)
        {
            try
            {
                var retVal = await _invoiceService.FetchInvoiceDetails(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] InvoiceRequestDto model)
        {
            try
            {
                var retVal = await _invoiceService.Update(id, model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


    }
}
