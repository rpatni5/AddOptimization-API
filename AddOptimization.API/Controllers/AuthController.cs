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

    
}
