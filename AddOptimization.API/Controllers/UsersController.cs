using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;

namespace UsersManagment.API.Controllers;
[Authorize]
public class UsersController : CustomApiControllerBase
{

    private readonly IUserService _usersService;
    public UsersController(ILogger<UsersController> logger, IUserService usersService) : base(logger)
    {
        _usersService = usersService;
    }


}