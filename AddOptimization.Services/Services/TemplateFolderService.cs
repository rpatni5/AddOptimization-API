using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Data.Repositories;
using AddOptimization.Services.Constants;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;


namespace AddOptimization.Services.Services
{
    public class TemplateFolderService : ITemplateFolderService
    {
        private readonly IGenericRepository<TemplateFolder> _folderRepository;
        private readonly ILogger<TemplateFolderService> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<SharedFolder> _sharedFolderRepository;
        private readonly IGenericRepository<GroupMember> _groupMemberRepository;

        public TemplateFolderService(IGenericRepository<TemplateFolder> folderRepository, ILogger<TemplateFolderService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IGenericRepository<SharedFolder> sharedFolderRepository, IGenericRepository<GroupMember> groupMemberRepository)
        {
            _folderRepository = folderRepository;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _sharedFolderRepository = sharedFolderRepository;
            _groupMemberRepository = groupMemberRepository;
        }

        private static TemplateFolderDto SelectTemplate(TemplateFolder x, List<SharedFolder> sharedFolders)
        {
            var entry = sharedFolders.FirstOrDefault(e => e.FolderId == x.Id);

            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return new TemplateFolderDto
            {
                Id = x.Id == null ? Guid.Empty : x.Id,
                IsDeleted = x.IsDeleted,
                CreatedBy = x.CreatedByUser != null ? x.CreatedByUser.FullName : string.Empty,
                CreatedAt = x.CreatedAt,
                Name = x.Name,
                Description = x.Description,
                Permission = entry != null ? entry.PermissionLevel : PermissionLevel.FullAccess.ToString()
            };
        }
        public async Task<ApiResult<List<TemplateFolderDto>>> GetAllTemplateFolders()
        {
            try
            {
                var currentUserId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                var groupIds = (await _groupMemberRepository.QueryAsync(x => !x.IsDeleted && x.UserId == currentUserId)).Select(x => x.GroupId.ToString()).Distinct().ToList();
                var sharedFolders = (await _sharedFolderRepository.QueryAsync(x => !x.IsDeleted && (x.SharedWithId == currentUserId.ToString() || groupIds.Contains(x.SharedWithId)), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser))).ToList();

                var entryIds = sharedFolders.Select(x => x.FolderId).Distinct().ToList();
                var entities = await _folderRepository.QueryAsync((e => !e.IsDeleted && (e.CreatedByUserId == currentUserId || entryIds.Contains(e.Id))), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser), orderBy: x => x.OrderBy(x => x.CreatedAt));
                var mappedEntities = entities.Select(x => SelectTemplate(x, sharedFolders)).ToList();
                return ApiResult<List<TemplateFolderDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        public async Task<ApiResult<bool>> Create(TemplateFolderDto model)
        {
            try
            {
                var isExists = await _folderRepository.IsExist(t => t.Name == model.Name, ignoreGlobalFilter: true);

                if (isExists)
                {
                    var errorMessage = "Folder already exists.";
                    return ApiResult<bool>.Failure(ValidationCodes.FolderAlreadyExists, errorMessage);
                }
                var entity = _mapper.Map<TemplateFolder>(model);
                await _folderRepository.InsertAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<TemplateFolderDto>> Update(Guid id, TemplateFolderDto model)
        {
            try
            {
                var entity = await _folderRepository.FirstOrDefaultAsync(t => t.Id == id, ignoreGlobalFilter: true);
                if (entity == null)
                {
                    return ApiResult<TemplateFolderDto>.NotFound("Folder not found");
                }
                _mapper.Map(model, entity);
                await _folderRepository.UpdateAsync(entity);
                var mappedEntity = _mapper.Map<TemplateFolderDto>(entity);
                return ApiResult<TemplateFolderDto>.Success(mappedEntity);
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
                var entity = await _folderRepository.FirstOrDefaultAsync(t => t.Id == id, ignoreGlobalFilter: true);
                if (entity == null)
                {
                    return ApiResult<bool>.NotFound("Folder");
                }
                entity.IsDeleted = true;
                await _folderRepository.UpdateAsync(entity);

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

