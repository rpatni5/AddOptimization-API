using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services;

public interface IRoleService
{
    Task<ApiResult<List<RoleDto>>> Search(PageQueryFiterBase filter);
    Task<ApiResult<RoleDto>> Update(Guid id, RoleCreateDto model);
    Task<ApiResult<RoleDto>> Create(RoleCreateDto model);
    Task<ApiResult<bool>> Delete(Guid id);
}
