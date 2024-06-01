using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services
{
    public interface ISchedulerEventHistory
    {
        Task<ApiResult<List<SchedulerEventHistoryDto>>> GetSchedulerEventHistory(Guid id);
    }
}

