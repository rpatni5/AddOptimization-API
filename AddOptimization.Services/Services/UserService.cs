using Microsoft.Extensions.Logging;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AddOptimization.Services.Services;
public class UserService : IUserService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly ILogger<UserService> _logger;
    public UserService(IGenericRepository<User> userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

}
