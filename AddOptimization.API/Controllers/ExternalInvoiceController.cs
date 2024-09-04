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

        public ExternalInvoiceController(ILogger<ExternalInvoiceController> logger, IExternalInvoiceService externalInvoiceService, CustomDataProtectionService customDataProtectionService) : base(logger)
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
                var retVal = await _externalInvoiceService.FetchExternalInvoiceDetails(id);
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
        public async Task<IActionResult> SendInvoiceApprovalEmailToAccountAdmin(int id)
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


        [HttpPost("send-email-to-customer/{id}")]
        public async Task<IActionResult> SendInvoiceApprovalEmailToCustomer(int id)
        {
            try
            {
                var retVal = await _externalInvoiceService.SendInvoiceApprovalEmailToCustomer(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


        [HttpPost("external-invoice-decline-request")]
        public async Task<IActionResult> DeclineRequest(ExternalInvoiceActionRequestDto model)
        {
            try
            {
                var retVal = await _externalInvoiceService.DeclineRequest(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
        [HttpGet("get-history/{id}")]
        public async Task<IActionResult> GetHistoryId(int id)
        {
            try
            {
                var retVal = await _externalInvoiceService.GetExternalInvoiceHistoryById(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}