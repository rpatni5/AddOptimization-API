using AddOptimization.API.Common;
using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using AddOptimization.Utilities.Models;
using AddOptimization.Utilities.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class QuoteController : CustomApiControllerBase
    {
        private readonly IQuoteService _quoteService;
        private readonly CustomDataProtectionService _customDataProtectionService;
        public QuoteController(ILogger<QuoteController> logger, IQuoteService quoteService, CustomDataProtectionService customDataProtectionService) : base(logger)
        {
            _quoteService = quoteService;
            _customDataProtectionService = customDataProtectionService;
        }

        [HttpPost("search")]
        public async Task<IActionResult> Get([FromBody] PageQueryFiterBase filters)
        {
            try
            {
                var retVal = await _quoteService.Search(filters);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


        [HttpPost]
        public async Task<IActionResult> Create(QuoteRequestDto model)
        {
            try
            {
                var retVal = await _quoteService.Create(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, QuoteRequestDto model)
        {
            try
            {
                var retVal = await _quoteService.Update(id, model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("get-quote-details/{id}")]
        public async Task<IActionResult> FetchItemConfDetails(long id)
        {
            try
            {
                var retVal = await _quoteService.FetchQuoteDetails(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("send-quote-to-customer/{quoteId}")]
        public async Task<IActionResult> SendQuoteEmailToCustomer(long quoteId)
        {
            try
            {
                var retVal = await _quoteService.SendQuoteEmailToCustomer(quoteId);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [AllowAnonymous]

        [HttpGet("customer-quote-details/{id}")]
        public async Task<IActionResult> GetCustomerQuoteDetails(string id)
        {
            try
            {

                var decryptedString = _customDataProtectionService.Decode(id);
                var st = decryptedString;
                if (!long.TryParse(decryptedString, out var decryptedId))
                {
                    throw new ArgumentException("Invalid decrypted ID format");
                }
                var retVal = await _quoteService.FetchQuoteDetails(decryptedId);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


        [HttpPost("convertQuoteToInvoice/{quoteId}")]
        public async Task<IActionResult> ConvertQuoteToInvoice(long quoteId)
        {
            try
            {
                var retVal = await _quoteService.ConvertInvoice(quoteId);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
