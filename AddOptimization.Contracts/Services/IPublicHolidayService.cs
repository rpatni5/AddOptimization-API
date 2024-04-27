using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Services;

public interface IPublicHolidayService
{
    Task<ApiResult<List<PublicHolidayDto>>> Search(PageQueryFiterBase filters);
    Task<ApiResult<bool>> Create(PublicHolidayDto model);
    Task<ApiResult<PublicHolidayDto>> Update(Guid id, PublicHolidayDto model);
    Task<ApiResult<bool>> Delete(Guid id);
    Task<ApiResult<PublicHolidayDto>> Get(Guid id);
    Task<ApiResult<List<PublicHolidayDto>>> GetByCountryId(Guid countryid);
    Task<ApiResult<List<CountryDto>>> GetAllCountry();


}

