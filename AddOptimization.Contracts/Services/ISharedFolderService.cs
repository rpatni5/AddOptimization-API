using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services
{
    public interface ISharedFolderService
    {

        Task<ApiResult<bool>> Create(SharedFolderRequestDto model);
        Task<ApiResult<List<SharedFolderResponseDto>>> GetSharedFolderDataBySharedId(Guid id);
        Task<ApiResult<bool>> Delete(Guid id);
        Task<ApiResult<List<SharedFolderResponseDto>>> Update(Guid id, FolderPermissionLevelDto model);
    }
}

