using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services
{
    public interface ISchedulerEventService
    {
        Task<PagedApiResult<SchedulersDto>> Search(PageQueryFiterBase filter);
     
        Task<ApiResult<bool>> Delete(Guid id);

        Task<ApiResult<bool>> Upsert(List<SchedulersDto> model);

    }
}
