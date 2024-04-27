using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services
{
    public interface ICountryCodeService
    {
        Task<ApiResult<List<CountryDto>>> GetCountries();

        Task<ApiResult<List<CountryDto>>> GetByCountryId(Guid countryid);
        Task<ApiResult<List<CountryDto>>> GetAllCountry();
    }
}

