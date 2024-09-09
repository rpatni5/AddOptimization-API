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
    Task<PagedApiResult<PublicHolidayResponseDto>> Search(PageQueryFiterBase filters);
    Task<ApiResult<PublicHolidayResponseDto>> Create(PublicHolidayRequestDto model);
    Task<ApiResult<PublicHolidayResponseDto>> Update(Guid id, PublicHolidayRequestDto model);
    Task<ApiResult<bool>> Delete(Guid id);
    Task<ApiResult<PublicHolidayResponseDto>> Get(Guid id);
    Task<ApiResult<List<PublicHolidayResponseDto>>> SearchAllPublicHoliday();




}

