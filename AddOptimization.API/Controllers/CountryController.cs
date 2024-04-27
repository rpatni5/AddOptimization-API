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


        [HttpGet("countries")]
        public async Task<IActionResult> GetCountries()
        {
            try
            {
                var result = await _countryCodeService.GetCountries();
                return HandleResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

    }
}

