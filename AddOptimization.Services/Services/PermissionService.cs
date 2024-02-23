using iText.StyledXmlParser.Jsoup.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Services.Constants;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Services.Services;

public class PermissionService: IPermissionService
{
    private readonly IGenericRepository<RolePermission> _rolePermissionRepository;
    private readonly IGenericRepository<Role> _roleRepository;
    private readonly IGenericRepository<Field> _fieldRepository;
   // private readonly IGenericRepository<Notification> _notificationRepository;
    private readonly IGenericRepository<ApplicationUser> _applicationUserRepository;
    private readonly IGenericRepository<Screen> _screenRepository;
    private readonly ILogger<PermissionService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _cache;
    public PermissionService(ILogger<PermissionService> logger, IHttpContextAccessor httpContextAccessor, IMemoryCache cache, IGenericRepository<RolePermission> rolePermissionRepository, IGenericRepository<Screen> screenRepository, IGenericRepository<ApplicationUser> applicationUserRepository, IGenericRepository<Role> roleRepository, IGenericRepository<Field> fieldRepository) //IGenericRepository<Notification> notificationRepository,
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _cache = cache;
        _rolePermissionRepository = rolePermissionRepository;
        _screenRepository = screenRepository;
       // _notificationRepository = notificationRepository;
        _applicationUserRepository = applicationUserRepository;
        _roleRepository = roleRepository;
        _fieldRepository = fieldRepository;
    }
    public async Task<ApiResult<PermissionConfigDto>> GetConfigData()
    {
        try
        {
            var retVal = new PermissionConfigDto();
            var roles = (await _roleRepository.QueryMappedAsync(r => new RoleDto
            {
                Name = r.Name,
                Id = r.Id
            }, r => !r.IsDeleted)).ToList();
            var screens = (await _screenRepository.QueryMappedAsync(r => new ScreenDto
            {
                Name = r.Name,
                Id = r.Id
            })).ToList();
            var fields = (await _fieldRepository.QueryMappedAsync(r => new FieldDto
            {
                Name = r.Name,
                FieldKey = r.FieldKey,
                Id = r.Id
            })).ToList();
            retVal.Roles = roles;
            retVal.Screens = screens;
            retVal.Fields = fields;
            return ApiResult<PermissionConfigDto>.Success(retVal);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    public async Task<ApiResult<List<PermissionDto>>> Search(PageQueryFiterBase filter)
    {
        try
        {
            Guid? roleId = null;
            filter.GetValue<Guid>("roleId", (v) =>
            {
                roleId = v;
            });
            var rolePermissions = (await _rolePermissionRepository.QueryMappedAsync(a => new
            {
                a.FieldId,
                a.Field,
                RoleName = a.Role == null ? null : a.Role.Name,
                a.RoleId,
                a.Id,
                Screen = a.Screen == null ? null : a.Screen.Name,
                a.ScreenId
            }, e => roleId == null || e.RoleId == roleId, include: entities => entities.Include(a => a.Role).Include(a => a.Field).Include(a => a.Screen))).ToList();
            var retVal = rolePermissions.GroupBy(r => new { r.RoleId, r.ScreenId }).Select(g =>
            {
                var firstItem = g.FirstOrDefault();
                var fieldsGroup = g.GroupBy(i => i.FieldId);
                var fields = fieldsGroup.Select(ag => new FieldDto
                {
                    Name = ag.FirstOrDefault().Field.Name,
                    FieldKey = ag.FirstOrDefault().Field.FieldKey,
                    Id = ag.Key
                }).ToList();
                return new PermissionDto
                {
                    RoleId = g.Key.RoleId,
                    RoleName = firstItem.RoleName,
                    ScreenId = g.Key.ScreenId,
                    Screen = firstItem.Screen,
                    Fields = fields
                };
            }).OrderBy(r => r.RoleName).ThenBy(r => r.Screen).ToList();
            return ApiResult<List<PermissionDto>>.Success(retVal);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    private async Task<List<ScreenDto>> GetUserScreensAccess()
    {
        try
        {
            var roles = GetCurrentUserRoles();
            if (!roles.Any())
            {
                return Enumerable.Empty<ScreenDto>().ToList();
            }
            if (roles.Contains(UserRoles.SuperAdmin))
            {
                return await GetSuperAdminAccess();
            }
            var rolePermissions = (await _rolePermissionRepository.QueryAsync(a => a.FieldId != null && roles.Contains(a.Role.Name), include: entities => entities.Include(e => e.Role).Include(e => e.Screen).Include(e => e.Field))).ToList();
            var retVal = rolePermissions.GroupBy(p => p.ScreenId).Select(g =>
            {
                var screen = g.First().Screen;
                return new ScreenDto
                {
                    Name = screen.Name,
                    Route = screen.Route,
                    ScreenKey = screen.ScreenKey,
                    Fields = g.Select(rp => new FieldDto { Name = rp.Field.Name, FieldKey = rp.Field.FieldKey }).ToList()
                };
            }).ToList();
            return retVal;
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    private async Task<List<ScreenDto>> GetSuperAdminAccess()
    {
        var globalFields = (await _fieldRepository.QueryMappedAsync(e => new FieldDto
        {
            Name = e.Name,
            FieldKey = e.FieldKey,
        }, e => e.ScreenId == null)).ToList();
        var screens = (await _screenRepository.QueryAsync(select: s => new Screen
        {
            Name = s.Name,
            Route = s.Route,
            ScreenKey = s.ScreenKey,
            Fields = s.Fields.Select(f => new Field
            {
                Name = f.Name,
                FieldKey = f.FieldKey
            }).ToList()
        }, include: entities => entities.Include(s => s.Fields))).ToList();
        return screens.Select(s =>
        {
            var retVal = new ScreenDto
            {
                Name = s.Name,
                Route = s.Route,
                ScreenKey = s.ScreenKey,
                Fields = s.Fields.Select(f => new FieldDto
                {
                    Name = f.Name,
                    FieldKey = f.FieldKey
                }).ToList()
            };
            retVal.Fields.AddRange(globalFields);
            return retVal;
        }).ToList();
    }

    public async Task<ApiResult<UserAccessDto>> GetUserAccess(UserAccessDto model)
    {
        try
        {
            var permissionVersion = model.PermissionsVersion;
            var previousRoles = model.Roles ?? new List<string>();
            var currentRoles = GetCurrentUserRoles();
            var currentUserId = _httpContextAccessor.HttpContext.GetCurrentUserId();
            bool isRoleUpdated = previousRoles.Count != currentRoles.Count || currentRoles.Any(r => !previousRoles.Contains(r)) || previousRoles.Any(r => !currentRoles.Contains(r));
           // var notificationCount = (await _notificationRepository.QueryMappedAsync(n => n.Id, n => n.IsRead != true && n.AppplicationUserId == currentUserId)).Count();
            var isEmailsEnabled = (await _applicationUserRepository.FirstOrDefaultAsync(u => u.Id == currentUserId))?.IsEmailsEnabled;
            var result = new UserAccessDto
            {
                PermissionsVersion = permissionVersion,
                Roles = previousRoles,
                UnreadNotificationCount =0,// notificationCount,
                IsEmailsEnabled = isEmailsEnabled ?? false
            };

            var currentPermissionVersion = _cache.Get<Guid?>(CacheKeys.PermissionVersion);
            if (isRoleUpdated || permissionVersion == null || currentPermissionVersion == null || permissionVersion != currentPermissionVersion)
            {
                var data = await GetUserScreensAccess();
                result.Screens = data;
                result.Roles = currentRoles;
                result.PermissionsVersion = _cache.GetOrCreate(CacheKeys.PermissionVersion, (c) =>
                {
                    return Guid.NewGuid();
                });
            }

            return ApiResult<UserAccessDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    private List<string> GetCurrentUserRoles()
    {
        return _httpContextAccessor.HttpContext.GetCurrentUserRoles();
    }


    public async Task<ApiResult<bool>> SaveRolePermissions(PermissionCreateDto model)
    {
        try
        {

            var existingEntities = (await _rolePermissionRepository.QueryAsync(r =>
            r.RoleId == model.RoleId && r.ScreenId == model.ScreenId
            )).ToList();
            var newRolePermissionEntities = model.Fields.Where(a => !existingEntities.Exists(e => e.FieldId == a.Id)).Select(a => new RolePermission
            {
                ScreenId = model.ScreenId,
                RoleId = model.RoleId,
                FieldId = a.Id,
                Id = Guid.NewGuid()
            }).ToList();
            var permissionsToDelete = existingEntities.Where(e => !model.Fields.Exists(f => f.Id == e.FieldId)).ToList();
            if (permissionsToDelete.Any())
            {
                await _rolePermissionRepository.BulkDeleteAsync(permissionsToDelete);
            }
            if (newRolePermissionEntities.Any())
            {
                await _rolePermissionRepository.BulkInsertAsync(newRolePermissionEntities);
            }
            _cache.Set(CacheKeys.PermissionVersion, Guid.NewGuid());
            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<bool> CheckPermissionForUser(string screens, string fields)
    {
        try
        {
            var ss = screens.Split(';').ToList();
            var sf = fields.Split(";").ToList();
            return await CheckPermissionForUser(ss, sf);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<bool> CheckPermissionForUser(List<string> screens, List<string> fields)
    {
        try
        {
            var roles = GetCurrentUserRoles();
            fields = fields?.Select(f => f.ToLower()).ToList();
            return roles.Contains(UserRoles.SuperAdmin) || await _rolePermissionRepository.IsExist(r => r.Role != null && roles.Contains(r.Role.Name) && screens.Contains(r.Screen.Name) && (fields == null || (r.Field != null && fields.Contains(r.Field.FieldKey.ToLower()))));
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<bool>> DeleteRolePermission(PermissionDeleteDto model)
    {
        try
        {

            var entities = (await _rolePermissionRepository.QueryAsync(r => r.RoleId == model.RoleId && r.ScreenId == model.ScreenId)).ToList();
            if (entities.Any())
            {
                await _rolePermissionRepository.BulkDeleteAsync(entities);
                _cache.Set(CacheKeys.PermissionVersion, Guid.NewGuid());
            }
            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
}