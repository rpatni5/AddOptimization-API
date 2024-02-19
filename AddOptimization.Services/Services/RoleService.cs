using Microsoft.Extensions.Logging;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Constants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using AddOptimization.Utilities.Models;
using AddOptimization.Contracts.Constants;

namespace AddOptimization.Services.Services;
public class RoleService : IRoleService
{
    private readonly IGenericRepository<Role> _roleRepository;
    private readonly ILogger<RoleService> _logger;
    private readonly IMapper _mapper;
    public RoleService(IGenericRepository<Role> roleRepository, ILogger<RoleService> logger, IMapper mapper)
    {
        _roleRepository = roleRepository;
        _logger = logger;
        _mapper = mapper;
    }


    public async Task<ApiResult<List<RoleDto>>> Search(PageQueryFiterBase filter)
    {
        try
        {
            var entities = await _roleRepository.QueryMappedAsync(s => new RoleDto
            {
                Id = s.Id,
                Name = s.Name,
                IsDeleted = s.IsDeleted,
                DepartmentId = s.DepartmentId,
                //DepartmentName=s.Department==null? null : s.Department.Name,
                UserCount=s.UserRoles.Count
            }, orderBy: (entities) => entities.OrderBy(c => c.Name),include:entities=> entities.Include(r=> r.UserRoles));
            filter.GetValue<bool>("activeRolesOnly", (val) =>
            {
                entities = entities.Where(e => e.IsDeleted == !val);
            });
            return ApiResult<List<RoleDto>>.Success(entities.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<RoleDto>> Create(RoleCreateDto model)
    {
        try
        {
            var isExists = await _roleRepository.IsExist(t =>t.Name.ToLower() == model.Name.ToLower());
            if (isExists)
            {
                return ApiResult<RoleDto>.EntityAlreadyExists("Role","name");
            }
            var entity = _mapper.Map<Role>(model);
            entity = await _roleRepository.InsertAsync(entity);
            var mappedEntity = _mapper.Map<RoleDto>(entity);
            return ApiResult<RoleDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<RoleDto>> Update(Guid id, RoleCreateDto model)
    {
        try
        {
            var isExists = await _roleRepository.IsExist(t => t.Id != id && t.Name.ToLower() == model.Name.ToLower());
            if (isExists)
            {
                return ApiResult<RoleDto>.EntityAlreadyExists("Role", "name");
            }

            var entity = await _roleRepository.FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null)
            {
                return ApiResult<RoleDto>.NotFound("Role");
            }
            if (entity.Name == UserRoles.SuperAdmin)
            {
                return ApiResult<RoleDto>.Failure(ValidationCodes.SystemRoleModificationNotAllowed);
            }
            _mapper.Map(model, entity);
            await _roleRepository.UpdateAsync(entity);
            var mappedEntity = _mapper.Map<RoleDto>(entity);
            return ApiResult<RoleDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<bool>> Delete(Guid id)
    {
        try
        {
            var entity = await _roleRepository.FirstOrDefaultAsync(t => t.Id == id, include: entities => entities.Include(e => e.RolePermissions).Include(e => e.UserRoles));
            if (entity == null)
            {
                return ApiResult<bool>.NotFound("Role");
            }
            if(entity.Name == UserRoles.SuperAdmin)
            {
                return ApiResult<bool>.Failure(ValidationCodes.SystemRoleModificationNotAllowed);
            }
            if (entity.RolePermissions.Any() || entity.UserRoles.Any())
            {
                return ApiResult<bool>.Failure(ValidationCodes.RoleInUseDeleteNotAllowed);
            }
            entity.IsDeleted = true;
            await _roleRepository.UpdateAsync(entity);
            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

}
