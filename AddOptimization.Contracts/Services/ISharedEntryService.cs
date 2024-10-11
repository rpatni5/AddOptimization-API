using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services
{
    public interface ISharedEntryService
    {
        Task<ApiResult<bool>> Create(SharedEntryRequestDto model);
        Task<ApiResult<List<SharedEntryResponseDto>>> GetSharedDataBySharedId(Guid id);
        Task<ApiResult<bool>> Delete(Guid id);
        Task<ApiResult<List<SharedEntryResponseDto>>> GetByUserId(int id , string filterType);
        Task<ApiResult<List<SharedEntryResponseDto>>> Update(Guid id, PermissionLevelDto model);

    }
}

