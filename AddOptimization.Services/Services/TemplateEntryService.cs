using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Services.Constants;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace AddOptimization.Services.Services
{
    public class TemplateEntryService : ITemplateEntryService
    {
        private readonly IGenericRepository<TemplateEntries> _templateEntryRepository;
        private readonly ILogger<TemplateEntryService> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository<ApplicationUser> _applicationUserRepository;
        private readonly IGenericRepository<Group> _groupRepository;
        private readonly IGenericRepository<GroupMember> _groupMemberRepository;
        private readonly ITemplatesService _templateService;
        private readonly IGenericRepository<SharedEntry> _sharedEntryRepository;
        private readonly IGenericRepository<SharedFolder> _sharedFolderRepository;

        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public TemplateEntryService(IGenericRepository<TemplateEntries> templateEntryRepository, ILogger<TemplateEntryService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IGenericRepository<ApplicationUser> applicationUserRepository, IGenericRepository<Group> groupRepository, ITemplatesService templateService, IGenericRepository<SharedEntry> sharedEntryRepository, IGenericRepository<SharedFolder> sharedFolderRepository ,IGenericRepository<GroupMember> groupMemberRepository)
        {
            _templateEntryRepository = templateEntryRepository;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _applicationUserRepository = applicationUserRepository;
            _groupRepository = groupRepository;
            _templateService = templateService;
            _sharedEntryRepository = sharedEntryRepository;
            _groupMemberRepository = groupMemberRepository;
            _sharedFolderRepository = sharedFolderRepository;
        }


        public async Task<ApiResult<bool>> Save(TemplateEntryDto model)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                var entity = _mapper.Map<TemplateEntries>(model);
                entity.UserId = userId;
                entity.FolderId = model.FolderId;
                entity.EntryData = JsonSerializer.Serialize(model.EntryData, jsonOptions);
                await _templateEntryRepository.InsertAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<List<TemplateEntryDto>>> Search(Guid? templateId, string textSearch = null)
        {
            try
            {
                var currentUserId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;

                var groupIds = (await _groupMemberRepository.QueryAsync(x => !x.IsDeleted && x.UserId == currentUserId)).Select(x => x.GroupId.ToString()).Distinct().ToList();

                var sharedEntries = (await _sharedEntryRepository.QueryAsync(x => !x.IsDeleted && (x.SharedWithId == currentUserId.ToString() || groupIds.Contains(x.SharedWithId)), include: entities => entities .Include(e => e.CreatedByUser) .Include(e => e.UpdatedByUser))).ToList();

                var entryIds = sharedEntries.Select(x => x.EntryId).Distinct().ToList();

                var sharedFolders = (await _sharedFolderRepository.QueryAsync(x => !x.IsDeleted && (x.SharedWithId == currentUserId.ToString() || groupIds.Contains(x.SharedWithId)), include: entities => entities.Include(e => e.TemplateFolder))).ToList();

                var folderIds = sharedFolders.Select(x => x.FolderId).Distinct().ToList();

                var entities = await _templateEntryRepository.QueryAsync(
                    e => !e.IsDeleted && (e.UserId == currentUserId || entryIds.Contains(e.Id) || folderIds.Contains(e.FolderId)),
                    include: entities => entities
                        .Include(e => e.CreatedByUser).Include(e => e.TemplateFolder).Include(e => e.Template)
                        .Include(e => e.UpdatedByUser)
                        .Include(e => e.ApplicationUser),
                    orderBy: x => x.OrderByDescending(x => x.CreatedAt)
                );

                if (templateId.HasValue)
                {
                    entities = entities.Where(e => e.TemplateId == templateId.Value);
                }

                if (!string.IsNullOrEmpty(textSearch))
                {
                    entities = entities.Where(x => x.Title.ToLower().Contains(textSearch.ToLower()));
                }

                var mappedEntities = entities.Select(x => SelectTemplate(x, sharedEntries , sharedFolders)).ToList();

                return ApiResult<List<TemplateEntryDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        private static TemplateEntryDto SelectTemplate(TemplateEntries x, List<SharedEntry> sharedEntries , List<SharedFolder> sharedFolders)
        {
            var entry = sharedEntries.FirstOrDefault(e => e.EntryId == x.Id);
            var sharedFolder = x.FolderId != null ? sharedFolders.FirstOrDefault(f => f.FolderId == x.FolderId) : null;
            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return new TemplateEntryDto
            {
                Id = x.Id == null ? Guid.Empty : x.Id,
                Title = x.Title,
                FolderId = x.FolderId,
                UserId = x.UserId,
                TemplateId = x.TemplateId,
                IsDeleted = x.IsDeleted,
                CreatedBy =  x.CreatedByUser != null ? x.CreatedByUser.FullName : string.Empty,
                CreatedAt = x.CreatedAt,
                EntryData = x.EntryData == null ? new EntryDataDto() : JsonSerializer.Deserialize<EntryDataDto>(x.EntryData, options),
                Permission = entry != null ? entry.PermissionLevel : sharedFolder != null ? sharedFolder.PermissionLevel :  PermissionLevel.FullAccess.ToString()
            };
        }

        public async Task<ApiResult<TemplateEntryDto>> Update(Guid id, TemplateEntryDto model)
        {
            try
            {
                var entity = await _templateEntryRepository.FirstOrDefaultAsync(e => e.Id == id);
                entity.FolderId = model.FolderId;
                entity.Title = model.Title;
                entity.EntryData = JsonSerializer.Serialize(model.EntryData, jsonOptions);
                await _templateEntryRepository.UpdateAsync(entity);
                var mappedEntity = _mapper.Map<TemplateEntryDto>(entity);
                return ApiResult<TemplateEntryDto>.Success(mappedEntity);
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
                var entity = await _templateEntryRepository.FirstOrDefaultAsync(t => t.Id == id, ignoreGlobalFilter: true);
                if (entity == null)
                {
                    return ApiResult<bool>.NotFound("Data");
                }

                entity.IsDeleted = true;
                await _templateEntryRepository.UpdateAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<TemplateEntryDto>> Get(Guid id)
        {
            try
            {
                var currentUserId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value.ToString();
                var sharedEntries = (await _sharedEntryRepository.QueryAsync(x => x.SharedWithId == currentUserId &&
            (x.EntryId == id))).ToList();
                var sharedFolders = (await _sharedFolderRepository.QueryAsync(x => !x.IsDeleted && (x.SharedWithId == currentUserId), include: entities => entities.Include(e => e.TemplateFolder))).ToList();


                var entity = (await _templateEntryRepository.QueryAsync(o => o.Id == id && !o.IsDeleted, ignoreGlobalFilter: true)).FirstOrDefault();
                if (entity == null)
                {
                    return ApiResult<TemplateEntryDto>.NotFound("Data");
                }
                var mappedEntity = SelectTemplate(entity, sharedEntries, sharedFolders);
                return ApiResult<TemplateEntryDto>.Success(mappedEntity);
            }

            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }



    }

}

