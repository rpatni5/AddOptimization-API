using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Services
{
    public interface ISchedulersService
    {
        Task<PagedApiResult<SchedulersDto>> Search(PageQueryFiterBase filter);
        Task<ApiResult<bool>> Create(SchedulersDto model);

        Task<ApiResult<SchedulersDto>> Update(Guid id, SchedulersDto model);
        Task<ApiResult<bool>> Delete(Guid id);


    }
}
