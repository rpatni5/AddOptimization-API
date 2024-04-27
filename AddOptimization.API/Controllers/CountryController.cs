using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AddOptimization.Contracts.Services;
using AddOptimization.API.Common;

namespace AddOptimization.API.Controllers
{

    [Authorize]
    public class CountryController : CustomApiControllerBase
    {
        private readonly ICountryCodeService _countryCodeService;
        public CountryController(ILogger<CountryController> logger, ICountryCodeService countryCodeService) : base(logger)
        {
            _countryCodeService = countryCodeService;
        }

        [HttpGet("get-by-countryid")]
        public async Task<IActionResult> GetByCountryId(Guid countryid)
        {
            try
            {
                var retVal = await _countryCodeService.GetByCountryId(countryid);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("get-all-countries")]
        public async Task<IActionResult> GetAllCountry()
        {
            try
            {
                var retVal = await _countryCodeService.GetAllCountry();
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

    }
}

