using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using AddOptimization.Utilities.Models;
using AddOptimization.Utilities.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class ExternalInvoiceController : CustomApiControllerBase
    {
        private readonly IExternalInvoiceService _externalInvoiceService;
        private readonly CustomDataProtectionService _customDataProtectionService;

        public ExternalInvoiceController(ILogger<ExternalInvoiceController> logger, IExternalInvoiceService externalInvoiceService ,CustomDataProtectionService customDataProtectionService): base(logger)
        {
            _externalInvoiceService = externalInvoiceService;
            _customDataProtectionService = customDataProtectionService;

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
        public async Task<IActionResult> FetchExternalInvoiceDetails(long id)
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
        public async Task<IActionResult> Update(long id, ExternalInvoiceRequestDto model)
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

        [HttpPost("send-email-to-account-admin/{id}")]
        public async Task<IActionResult> SendInvoiceApprovalEmailToAccountAdmin(long id)
        {
            try
            {
                var retVal = await _externalInvoiceService.SendInvoiceApprovalEmailToAccountAdmin(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


        [AllowAnonymous]

        [HttpGet("customer-invoice-details/{id}")]
        public async Task<IActionResult> GetCustomerInvoiceDetails(string id)
        {
            try
            {

                var decryptedString = _customDataProtectionService.Decode(id);
                var st = decryptedString;
                if (!long.TryParse(decryptedString, out var decryptedId))
                {
                    throw new ArgumentException("Invalid decrypted ID format");
                }
                var retVal = await _externalInvoiceService.FetchInvoiceDetails(decryptedId);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}