using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Dto;
using Microsoft.AspNetCore.Authorization;
using AddOptimization.Contracts.Constants;

namespace AddOptimization.API.Controllers;

public class AuthController : CustomApiControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(ILogger<AuthController> logger, IAuthService authService) : base(logger)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        try
        {
            var retVal = await _authService.Login(model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
    [HttpPost("forgotpassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
        try
        {
            var retVal = await _authService.SendForgotPasswordLink(model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        try
        {
            var retVal = await _authService.ResetPassword(model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
    [HttpPost("resetpassword/admin")]
    [Authorize]
    [HasPermission(ScreenKeys.Users, GlobalFields.Update)]
    public async Task<IActionResult> ResetPasswordAdmin([FromBody] ResetPasswordDto model)
    {
        try
        {
            var retVal = await _authService.ResetPassword(model, true);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("validatedResetPasswordToken")]
    public async Task<IActionResult> ValidatedResetPasswordToken(ResetPasswordDto model)
    {
        try
        {
            var retVal = await _authService.ValidateResetPasswordToken(model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var retVal = await _authService.Logout();
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("refreshToken/{refreshToken}")]
    public async Task<IActionResult> RefreshToken(Guid refreshToken)
    {
        try
        {
            var retVal = await _authService.RefreshToken(refreshToken);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }


}
