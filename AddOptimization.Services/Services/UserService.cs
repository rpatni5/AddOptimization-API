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
    public async Task<ApiResult<List<UserSummaryDto>>> Search()
    {
        try
        {
            var entities = await _userRepository.QueryAsync();
            var usersList = entities.Select(s => new UserSummaryDto
            {
                Id = s.Id,
                FullName = s.FullName
            }).OrderBy(x => x.FullName).ToList();

            return ApiResult<List<UserSummaryDto>>.Success(usersList);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
}
