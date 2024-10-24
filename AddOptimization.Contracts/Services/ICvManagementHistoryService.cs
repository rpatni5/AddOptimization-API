using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services
{
    public interface ICvManagementHistoryService
    {
        Task<ApiResult<List<CvEntryHistoryDto>>> GetCvHistory(Guid id);
        Task<ApiResult<CvEntryHistoryDto>> GetHistoryDetailsById(Guid id);
    }
}

