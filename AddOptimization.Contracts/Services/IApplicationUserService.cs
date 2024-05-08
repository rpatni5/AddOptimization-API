using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services;

public interface IApplicationUserService
{
    Task<ApiResult<List<ApplicationUserDto>>> Search();
    Task<ApiResult<bool>> Create(UserCreateDto model);
    Task<ApiResult<bool>> Update(int userId, UserUpdateDto model);
    Task<ApiResult<List<ApplicationUserSummaryDto>>> SearchSummary(PageQueryFiterBase filters);
    Task<ApiResult<bool>> ToogleActivationStatus(int id);
    Task<ApiResult<bool>> ToggleEmailsEnabled(int userId);
    Task<ApiResult<List<ApplicationUserDto>>> GetAccountAdmins();
    Task<ApiResult<List<ApplicationUserDto>>> GetEmployee();
}
