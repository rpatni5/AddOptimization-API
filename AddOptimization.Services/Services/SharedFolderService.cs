using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
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
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace AddOptimization.Services.Services
{
    public class SharedFolderService : ISharedFolderService
    {
        private readonly IGenericRepository<SharedFolder> _sharedFolderRepository;
        private readonly ILogger<SharedFolderService> _logger;
        private readonly IMapper _mapper;
        private readonly ICreditCardService _creditCardService;
        private readonly IGenericRepository<ApplicationUser> _applicationUserRepository;
        private readonly IGenericRepository<Group> _groupRepository;

        public SharedFolderService(IGenericRepository<SharedFolder> sharedFolderRepository, ILogger<SharedFolderService> logger, IMapper mapper, IGenericRepository<ApplicationUser> applicationUserRepository, IGenericRepository<Group> groupRepository)
        {
            _sharedFolderRepository = sharedFolderRepository;
            _logger = logger;
            _mapper = mapper;
            _applicationUserRepository = applicationUserRepository;
            _groupRepository = groupRepository;
        }

        public async Task<ApiResult<bool>> Create(SharedFolderRequestDto model)
        {
            try
            {

                var sharedFolders = new List<SharedFolder>();
                foreach (var item in model.sharedFolder)
                {
                    var sharedFolder = new SharedFolder
                    {
                        Id = Guid.NewGuid(),
                        FolderId = model.FolderId,
                        SharedByUserId = model.SharedByUserId,
                        SharedWithId = item.Id,
                        SharedWithType = item.Type,
                        PermissionLevel = model.PermissionLevel,
                        SharedDate = model.SharedDate
                    };

                    sharedFolders.Add(sharedFolder);
                }

                await _sharedFolderRepository.BulkInsertAsync(sharedFolders);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<List<SharedFolderResponseDto>>> GetSharedFolderDataBySharedId(Guid id)
        {
            try
            {
                var entities = (await _sharedFolderRepository.QueryAsync(o => o.FolderId == id && !o.IsDeleted, include: entities => entities.Include(e => e.ApplicationUser), ignoreGlobalFilter: true));
                if (entities == null)
                {
                    return ApiResult<List<SharedFolderResponseDto>>.NotFound("details");
                }
                var users = await _applicationUserRepository.QueryAsync((e => e.IsActive));
                var groups = await _groupRepository.QueryAsync((e => !e.IsDeleted));
                var mappedEntity = _mapper.Map<List<SharedFolderResponseDto>>(entities);

                foreach (var entry in mappedEntity)
                {
                    if (entry.SharedWithType == "user")
                    {
                        var user = users.FirstOrDefault(u => u.Id.ToString() == entry.SharedWithId);
                        entry.SharedWithName = user?.FullName ?? "Unknown User";
                    }
                    else if (entry.SharedWithType == "group")
                    {
                        var group = groups.FirstOrDefault(g => g.Id.ToString() == entry.SharedWithId);
                        entry.SharedWithName = group?.Name ?? "Unknown Group";
                    }
                }

                return ApiResult<List<SharedFolderResponseDto>>.Success(mappedEntity);
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
                var entity = await _sharedFolderRepository.FirstOrDefaultAsync(t => t.Id == id, ignoreGlobalFilter: true);
                if (entity == null)
                {
                    return ApiResult<bool>.NotFound(" Details");
                }

                entity.IsDeleted = true;
                await _sharedFolderRepository.UpdateAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<List<SharedFolderResponseDto>>> Update(Guid id, FolderPermissionLevelDto model)
        {
            try
            {
                var sharedFolders = await _sharedFolderRepository.QueryAsync(e => e.FolderId == id && !e.IsDeleted, ignoreGlobalFilter: true);
                foreach (var item in model.PermissionLevelFolder)
                {
                    var sharedFolder = sharedFolders.FirstOrDefault(e => e.Id == item.Id);
                    if (sharedFolder != null)
                    {
                        sharedFolder.PermissionLevel = item.PermissionLevel;
                        await _sharedFolderRepository.UpdateAsync(sharedFolder);
                    }
                }
                var mappedEntities = _mapper.Map<List<SharedFolderResponseDto>>(sharedFolders);
                return ApiResult<List<SharedFolderResponseDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
    }
}

