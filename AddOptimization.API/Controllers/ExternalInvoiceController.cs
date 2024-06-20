using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class ExternalInvoiceController : CustomApiControllerBase
    {
        private readonly IExternalInvoiceService _externalInvoiceService;
        public ExternalInvoiceController(ILogger<ExternalInvoiceController> logger, IExternalInvoiceService externalInvoiceService) : base(logger)
        {
            _externalInvoiceService = externalInvoiceService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(ExternalInvoiceRequestDto model)
        {
            try
            {
                var retVal = await _externalInvoiceService.Create(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("search")]
        public async Task<IActionResult> Get([FromBody] PageQueryFiterBase filters)
        {
            try
            {
                var retVal = await _externalInvoiceService.Search(filters);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


        [HttpGet("get-external-invoice-details/{id}")]
        public async Task<IActionResult> FetchExternalInvoiceDetails(int id)
        {
            try
            {
                var retVal = await _externalInvoiceService.FetchInvoiceDetails(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ExternalInvoiceRequestDto model)
        {
            try
            {
                var retVal = await _externalInvoiceService.Update(id, model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}