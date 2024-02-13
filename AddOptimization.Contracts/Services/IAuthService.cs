using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services;

public interface IAuthService
{
    Task<ApiResult<bool>> Logout(int? applicationUserId=null);
}
