using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Models;
using AutoMapper;
using iText.StyledXmlParser.Jsoup.Nodes;
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


        public async Task<ApiResult<bool>> Create(CombineGroupModelDto model)
        {
            try
            {
                var currentUserId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                var isExists = await _groupRepository.IsExist(t => !t.IsDeleted && ( t.Name == model.group.Name && t.CreatedByUserId == currentUserId), ignoreGlobalFilter: true);

                if (isExists)
                {
                    var errorMessage = "Group already exists.";
                    return ApiResult<bool>.Failure(ValidationCodes.GroupAlreadyExists, errorMessage);
                }

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
                var currentUserId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                var entities = await _groupRepository.QueryAsync((e => (!e.IsDeleted) && (e.CreatedByUserId == currentUserId)), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser), orderBy: x => x.OrderBy(x => x.Name));
                var mappedEntities = _mapper.Map<List<GroupDto>>(entities);
                return ApiResult<List<GroupDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<CombineGroupModelDto>> GetGroupAndMembersByGroupId(Guid groupId)
        {
            try
            {
                var groupEntity = await _groupRepository.FirstOrDefaultAsync(t => t.Id == groupId, ignoreGlobalFilter: true);

                var memberEntities = (await _groupMemberRepository.QueryAsync(o => o.GroupId == groupId && !o.IsDeleted, include: entities => entities.Include(e => e.ApplicationUser), ignoreGlobalFilter: true));
                if (memberEntities == null && groupEntity == null)
                {
                    return ApiResult<CombineGroupModelDto>.NotFound("member");
                }

                var users = await _applicationUserRepository.QueryAsync(e => e.IsActive);
                var groupDto = _mapper.Map<GroupDto>(groupEntity);
                var memberDtos = _mapper.Map<List<GroupMemberDto>>(memberEntities);

                foreach (var member in memberDtos)
                {
                    var user = users.FirstOrDefault(u => u.Id == member.UserId);
                    member.UserName = user?.FullName ?? "Unknown User";
                }
                var combinedDto = new CombineGroupModelDto
                {
                    group = groupDto,
                    groupMembers = memberDtos
                };

                return ApiResult<CombineGroupModelDto>.Success(combinedDto);
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


        public async Task<ApiResult<bool>> Update(Guid id, CombineGroupModelDto model)
        {
            try
            {
                var currentUserId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                var groupEntity = await _groupRepository.FirstOrDefaultAsync(t => t.Id == id, ignoreGlobalFilter: true);
                if (groupEntity == null)
                {
                    return ApiResult<bool>.NotFound("Group not found.");
                }
                var isExists = await _groupRepository.IsExist(t => (!t.IsDeleted) && t.Id != id && (t.Name == model.group.Name && t.CreatedByUserId == currentUserId), ignoreGlobalFilter: true);

                if (isExists)
                {
                    var errorMessage = "Group already exists.";
                    return ApiResult<bool>.Failure(ValidationCodes.GroupAlreadyExists, errorMessage);
                }


                _mapper.Map(model.group, groupEntity);
                await _groupRepository.UpdateAsync(groupEntity);

                var existingGroupMembers = await _groupMemberRepository.QueryAsync(gm => gm.GroupId == id && !gm.IsDeleted, ignoreGlobalFilter: true);
                foreach (var memberDto in model.groupMembers)
                {
                    var existingMember = existingGroupMembers.FirstOrDefault(m => m.UserId == memberDto.UserId);
                    if (existingMember != null)
                    {
                        existingMember.JoinedDate = memberDto.JoinedDate;
                        await _groupMemberRepository.UpdateAsync(existingMember);
                    }
                    else
                    {
                        var newMember = new GroupMember
                        {
                            Id = Guid.NewGuid(),
                            GroupId = id,
                            UserId = memberDto.UserId,
                            JoinedDate = memberDto.JoinedDate,
                            IsDeleted = false,

                        };
                        await _groupMemberRepository.InsertAsync(newMember);
                    }
                    foreach (var existMember in existingGroupMembers)
                    {
                        if (!model.groupMembers.Any(m => m.UserId == existMember.UserId))
                        {
                            existMember.IsDeleted = true;
                            await _groupMemberRepository.UpdateAsync(existingMember);
                        }
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


        public async Task<ApiResult<bool>> DeleteGroupMember(Guid id)
        {
            try
            {
                var members = await _groupMemberRepository.QueryAsync(t => t.Id == id, ignoreGlobalFilter: true);
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

