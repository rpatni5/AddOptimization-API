using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services
{
    public interface ICountryService
    {
        Task<ApiResult<CountryDto>> GetCountriesById(Guid countryId);
        Task<ApiResult<List<CountryDto>>> GetAllCountries();
    }
}

