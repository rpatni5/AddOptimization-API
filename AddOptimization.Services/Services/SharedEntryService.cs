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


namespace AddOptimization.Services.Services
{
    public class SharedEntryService : ISharedEntryService
    {
        private readonly IGenericRepository<SharedEntry> _sharedEntryRepository;
        private readonly ILogger<SharedEntryService> _logger;
        private readonly IMapper _mapper;
        private readonly ICreditCardService _creditCardService;
        private readonly IGenericRepository<ApplicationUser> _applicationUserRepository;
        private readonly IGenericRepository<Group> _groupRepository;

        public SharedEntryService(IGenericRepository<SharedEntry> sharedEntryRepository, ILogger<SharedEntryService> logger, IMapper mapper, IGenericRepository<ApplicationUser> applicationUserRepository, IGenericRepository<Group> groupRepository)
        {
            _sharedEntryRepository = sharedEntryRepository;
            _logger = logger;
            _mapper = mapper;
            _applicationUserRepository = applicationUserRepository;
            _groupRepository = groupRepository;
        }

        public async Task<ApiResult<bool>> Create(SharedEntryRequestDto model)
        {
            try
            {

                var sharedEntries = new List<SharedEntry>();
                foreach (var item in model.sharedField)
                {
                    var sharedEntry = new SharedEntry
                    {
                        Id = Guid.NewGuid(),
                        EntryId = model.EntryId,
                        SharedByUserId = model.SharedByUserId,
                        SharedWithId = item.Id,
                        SharedWithType = item.Type,
                        PermissionLevel = model.PermissionLevel,
                        SharedDate = model.SharedDate
                    };

                    sharedEntries.Add(sharedEntry);
                }

                await _sharedEntryRepository.BulkInsertAsync(sharedEntries);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<ApiResult<List<SharedEntryResponseDto>>> GetSharedDataBySharedId(Guid id)
        {
            try
            {
                var entities = (await _sharedEntryRepository.QueryAsync(o => o.EntryId == id && !o.IsDeleted,include : entities => entities.Include(e => e.ApplicationUser), ignoreGlobalFilter: true));
                if (entities == null)
                {
                    return ApiResult<List<SharedEntryResponseDto>>.NotFound("details");
                }
                var users = await _applicationUserRepository.QueryAsync((e => e.IsActive));
                var groups = await _groupRepository.QueryAsync((e => !e.IsDeleted));
                var mappedEntity = _mapper.Map<List<SharedEntryResponseDto>>(entities);

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

                return ApiResult<List<SharedEntryResponseDto>>.Success(mappedEntity);
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
                var entity = await _sharedEntryRepository.FirstOrDefaultAsync(t => t.Id == id, ignoreGlobalFilter: true);
                if (entity == null)
                {
                    return ApiResult<bool>.NotFound("Card Details");
                }

                entity.IsDeleted = true;
                await _sharedEntryRepository.UpdateAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<List<SharedEntryResponseDto>>> Update(Guid id, PermissionLevelDto model)
        {
            try
            {
                var sharedEntries = await _sharedEntryRepository.QueryAsync(e => e.EntryId == id && !e.IsDeleted, ignoreGlobalFilter: true);
                foreach (var item in model.PermissionLevelEntries)
                {
                    var sharedEntry = sharedEntries.FirstOrDefault(e => e.Id == item.Id);
                    if (sharedEntry != null)
                    {
                        sharedEntry.PermissionLevel = item.PermissionLevel;
                        await _sharedEntryRepository.UpdateAsync(sharedEntry);
                    }
                }
                var mappedEntities = _mapper.Map<List<SharedEntryResponseDto>>(sharedEntries);
                return ApiResult<List<SharedEntryResponseDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

    }



}

