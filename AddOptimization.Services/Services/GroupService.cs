using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace AddOptimization.Services.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGenericRepository<Group> _groupRepository;
        private readonly IGenericRepository<GroupMember> _groupMemberRepository;
        private readonly ILogger<GroupService> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository<ApplicationUser> _applicationUserRepository;

        public GroupService(IGenericRepository<Group> groupRepository, ILogger<GroupService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IGenericRepository<ApplicationUser> applicationUserRepository, IGenericRepository<GroupMember> groupMemberRepository)
        {
            _groupRepository = groupRepository;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _applicationUserRepository = applicationUserRepository;
            _groupMemberRepository = groupMemberRepository;
        }


        public async Task<ApiResult<bool>> Create(CombineGroupModelRequestDto model)
        {
            try
            {
                var groupEntity = _mapper.Map<Group>(model.group);
                await _groupRepository.InsertAsync(groupEntity);

                var groupMembers = new List<GroupMember>();

                foreach (var member in model.groupMembers)
                {
                    var groupMember = new GroupMember
                    {
                        Id = Guid.NewGuid(),
                        GroupId = groupEntity.Id,
                        UserId = member.UserId,
                        JoinedDate = member?.JoinedDate,
                        IsDeleted = false,
                    };
                    groupMembers.Add(groupMember);
                }
                await _groupMemberRepository.BulkInsertAsync(groupMembers);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<ApiResult<List<GroupDto>>> GetAllGroups()
        {
            try
            {
                var entities = await _groupRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser));
                var mappedEntities = _mapper.Map<List<GroupDto>>(entities);
                return ApiResult<List<GroupDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<List<GroupMemberDto>>> GetGroupMembersByGroupId(Guid groupId)
        {
            try
            {
                var entities = (await _groupMemberRepository.QueryAsync(o => o.Id == groupId && !o.IsDeleted, ignoreGlobalFilter: true));
                if (entities == null)
                {
                    return ApiResult<List<GroupMemberDto>>.NotFound("member");
                }
                var mappedEntity = _mapper.Map<List<GroupMemberDto>>(entities);

                return ApiResult<List<GroupMemberDto>>.Success(mappedEntity);
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
                var groupEntity = await _groupRepository.FirstOrDefaultAsync(t => t.Id == id, ignoreGlobalFilter: true);
                if (groupEntity == null)
                {
                    return ApiResult<bool>.NotFound("Group");
                }
                groupEntity.IsDeleted = true;
                await _groupRepository.UpdateAsync(groupEntity);

                var members = await _groupMemberRepository.QueryAsync(t => t.GroupId == id, ignoreGlobalFilter: true);
                var memberList = members.ToList();
                if (memberList.Any())
                {
                    foreach (var member in memberList)
                    {
                        member.IsDeleted = true;
                        await _groupMemberRepository.UpdateAsync(member);
                    }
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

}

