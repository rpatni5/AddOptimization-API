using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services;

public interface IAuthService
{    
    Task<ApiResult<AuthResponseDto>> Login(LoginDto model);
    Task<ApiResult<AuthResponseDto>> RefreshToken(Guid refreshToken);
    Task<ApiResult<bool>> Logout(int? applicationUserId = null);
    Task<ApiResult<bool>> SendForgotPasswordLink(ForgotPasswordDto model);
    Task<ApiResult<bool>> ResetPassword(ResetPasswordDto model, bool? isAdminMode = null);
    Task<ApiResult<bool>> ValidateResetPasswordToken(ResetPasswordDto model);
}
