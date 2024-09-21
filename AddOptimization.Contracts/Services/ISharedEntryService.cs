using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services
{
    public interface ISharedEntryService
    {
        Task<ApiResult<bool>> Create(SharedEntryRequestDto model);
        Task<ApiResult<List<SharedEntryResponseDto>>> GetSharedDataBySharedId(Guid id);
    }
}

