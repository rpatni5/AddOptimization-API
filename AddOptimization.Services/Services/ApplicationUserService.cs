using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Logging;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using Microsoft.EntityFrameworkCore;
using AddOptimization.Utilities.Models;
using AddOptimization.Utilities.Enums;
using AddOptimization.Utilities.Constants;
using AddOptimization.Contracts.Constants;

namespace AddOptimization.Services.Services;

public class ApplicationUserService : IApplicationUserService
{
    private readonly ILogger<ApplicationUserService> _logger;
    private readonly IGenericRepository<ApplicationUser> _applicationUserRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<UserRole> _userRoleRepository;
    private readonly IAuthService _authService;
    public ApplicationUserService(ILogger<ApplicationUserService> logger, IGenericRepository<ApplicationUser> applicationUserRepository, IMapper mapper, IAuthService authService, IUnitOfWork unitOfWork, IGenericRepository<UserRole> userRoleRepository)
    {
        _logger = logger;
        _applicationUserRepository = applicationUserRepository;
        _mapper = mapper;
        _authService = authService;
        _unitOfWork = unitOfWork;
        _userRoleRepository = userRoleRepository;
    }
    public async Task<ApiResult<List<ApplicationUserDto>>> Search()
    {
        try
        {
            var entities = await _applicationUserRepository.QueryMappedAsync(s => new ApplicationUserDto
            {
                Id = s.Id,
                FullName = s.FullName,
                FirstName=s.FirstName,
                UserName = s.UserName,
                LastName=s.LastName,
                Email=s.Email,
                IsActive=s.IsActive,
                IsLocked=s.IsLocked,
                IsEmailsEnabled =s.IsEmailsEnabled,
                Roles=s.UserRoles.Select(ur=>new RoleDto
                {
                    Id=ur.RoleId,
                    Name=ur.Role.Name
                }).ToList()
            }, orderBy: (entities) => entities.OrderBy(c => c.FullName),include:entities=> entities.Include(e=> e.UserRoles).ThenInclude(ur=> ur.Role));
            return ApiResult<List<ApplicationUserDto>>.Success(entities.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    public async Task<ApiResult<List<ApplicationUserSummaryDto>>> SearchSummary(PageQueryFiterBase filters)
    {
        try
        {
            var onlyAccountManagers = false;
            filters.GetValue<bool>("onlyAccountManagers", (v) =>
            {
                onlyAccountManagers = v;
            });
            var entities = await _applicationUserRepository.QueryMappedAsync(s => new ApplicationUserSummaryDto
            {
                Id = s.Id,
                FullName = s.FullName,
                Email = s.Email
            },e=> e.IsActive && (!onlyAccountManagers || e.UserRoles.Any(ur=> ur.Role.Name==UserRoles.AccountManager)), orderBy: (entities) => entities.OrderBy(c => c.FullName));
            filters.GetValue<string>("fullname", (v) =>
            {
                    entities = entities.Where(e => e.FullName != null && e.FullName.ToLower().Contains(v.ToLower()));
            },operatorType:OperatorType.contains);
           
            return ApiResult<List<ApplicationUserSummaryDto>>.Success(entities.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<bool>> Create(UserCreateDto model)
    {
        try
        {
            var existingEntity = await _applicationUserRepository.FirstOrDefaultAsync(u => (model.UserName != null && u.UserName.ToLower() == model.UserName.ToLower()) || (model.Email.ToLower()==u.Email.ToLower()));
            if (existingEntity != null)
            {
                return ApiResult<bool>.Failure(ValidationCodes.EmailUserNameAlreadyExists);
            }
            var entity = _mapper.Map<ApplicationUser>(model);
            var passwordHasher = new PasswordHasher();
            string hashedPassword = passwordHasher.HashPassword(model.Password);
            entity.Password = hashedPassword;
            entity.IsActive = true;
            entity.FullName = model.FirstName;
            entity.UserName = model.UserName ?? model.Email;
            if (model.LastName != null)
            {
                entity.FullName = $"{model.FirstName} {model.LastName}";
            }
            await _applicationUserRepository.InsertAsync(entity);
           
            return  ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    public async Task<ApiResult<bool>> Update(int userId, UserUpdateDto model)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var entity=await _applicationUserRepository.FirstOrDefaultAsync(u=> u.Id == userId);
            if (entity == null)
            {
                return ApiResult<bool>.NotFound("User");
            }
            entity.FirstName = model.FirstName;
            entity.LastName = model.LastName;
            entity.FullName =string.IsNullOrEmpty(model.LastName)?model.FirstName: $"{model.FirstName} {model.LastName}";
            await _applicationUserRepository.UpdateAsync(entity);
            var roles = model.Roles;
            var currentRoles = (await _userRoleRepository.QueryAsync(ur => ur.UserId == userId)).ToList();
            var entitiesToDelete = currentRoles.Where(ur =>!roles.Contains(ur.RoleId)).ToList();
            if (entitiesToDelete.Any())
            {
                await _userRoleRepository.BulkDeleteAsync(entitiesToDelete);
            }
            var entitiesToInsert = roles?.Where(r => !currentRoles.Exists(ur => ur.RoleId == r)).Select(r => new UserRole
            {
                UserId = userId,
                RoleId = r
            }).ToList();
            if (entitiesToInsert !=null && entitiesToInsert.Any())
            {
                await _userRoleRepository.BulkInsertAsync(entitiesToInsert);
            }         
            await _unitOfWork.CommitTransactionAsync();
            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<bool>> ToogleActivationStatus(int id)
    {
        try
        {
            var entity = await _applicationUserRepository.FirstOrDefaultAsync(u => u.Id==id,disableTracking:false);
            if (entity==null)
            {
                return ApiResult<bool>.NotFound($"User");
            }
            entity.IsActive = !entity.IsActive;
            await _applicationUserRepository.UpdateAsync(entity);
            if(!entity.IsActive)
            {
                await _authService.Logout(id);
            }
            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    public async Task<ApiResult<bool>> ToggleEmailsEnabled(int userId)
    {
        try
        {
            var entity = await _applicationUserRepository.FirstOrDefaultAsync(u => u.Id == userId, disableTracking: false);
            if (entity == null)
            {
                return ApiResult<bool>.NotFound($"User");
            }
            entity.IsEmailsEnabled = !(entity.IsEmailsEnabled ?? false);
            await _applicationUserRepository.UpdateAsync(entity);
            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

}
