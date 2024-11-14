using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services
{
    public interface ISharedEntryService
    {
        Task<ApiResult<bool>> Create(SharedEntryRequestDto model);
        Task<ApiResult<bool>> Delete(Guid id);
        Task<ApiResult<List<TemplateEntryDto>>> GetByUserId(int id, string filterType);
        Task<ApiResult<bool>> Update(Guid id, PermissionLevelDto model);
        Task<ApiResult<List<TemplateEntryDto>>> GetSharedData(Guid id);


    }
}

