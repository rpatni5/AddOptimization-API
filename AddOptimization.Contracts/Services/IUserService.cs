using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services;

public interface IUserService
{
    Task<ApiResult<List<UserSummaryDto>>> Search();
}
