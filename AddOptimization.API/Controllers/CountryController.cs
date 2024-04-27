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
        private readonly ICountryService _countryService;
        public CountryController(ILogger<CountryController> logger, ICountryService countryService) : base(logger)
        {
            _countryService = countryService;
        }

        [HttpGet("get-by-countryid")]
        public async Task<IActionResult> GetByCountryId(Guid countryId)
        {
            try
            {
                var retVal = await _countryService.GetCountriesById(countryId);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("get-all-countries")]
        public async Task<IActionResult> GetAllCountries()
        {
            try
            {
                var retVal = await _countryService.GetAllCountries();
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

    }
}

