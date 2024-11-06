using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AutoMapper;
using iText.StyledXmlParser.Jsoup.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace AddOptimization.Services.Services
{
    public class CvManagementHistoryService : ICvManagementHistoryService
    {
        private readonly IGenericRepository<CvEntryHistory> _cvEntryHistoryRepository;
        private readonly IGenericRepository<CvEntry> _cvEntryRepository;
        private readonly ILogger<CvManagementHistoryService> _logger;
        private readonly IMapper _mapper;

        private readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public CvManagementHistoryService(IGenericRepository<CvEntryHistory> cvEntryHistoryRepository, IGenericRepository<CvEntry> cvEntryRepository ,ILogger<CvManagementHistoryService> logger, IMapper mapper)
        {
            _cvEntryHistoryRepository = cvEntryHistoryRepository;
            _cvEntryRepository = cvEntryRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResult<List<CvEntryHistoryDto>>> GetCvHistory(Guid id)
        {
            try
            {
                var entities = await _cvEntryHistoryRepository
                    .QueryAsync(
                        x => x.CVEntryId == id && !x.IsDeleted,
                        orderBy: q => q.OrderByDescending(e => e.CreatedAt),
                        include: entities => entities
                          
                            .Include(e => e.CreatedByUser)
                    );

                var entityList = entities.ToList();

                if (entityList.Count > 5)
                {
                    var entitiesToDelete = entityList.Skip(5).ToList();
                    foreach (var entity in entitiesToDelete)
                    {
                        await _cvEntryHistoryRepository.DeleteAsync(entity);
                    }
                }

                var latestEntities = entityList.Take(5).ToList();

                var cvHistoryDtos = new List<CvEntryHistoryDto>();

                foreach (var entity in latestEntities)
                {
                    CvEntryDataDto entryData = new CvEntryDataDto(); 

                    if (!string.IsNullOrWhiteSpace(entity.EntryData))
                    {
                        try
                        {
                            entryData = JsonSerializer.Deserialize<CvEntryDataDto>(entity.EntryData, jsonOptions);
                        }
                        catch (JsonException)
                        {
                        }
                    }
                    cvHistoryDtos.Add(new CvEntryHistoryDto
                    {
                        Id = entity.Id,
                        CVEntryId = entity.CVEntryId,
                        EntryData = entity.EntryData,
                        EntryHistoryData = entryData,
                        IsDeleted = entity.IsDeleted,
                        CreatedAt = entity.CreatedAt,
                        CreatedBy = entity.CreatedByUser?.FullName ?? string.Empty,
                        UpdatedAt = entity.UpdatedAt,
                        UpdatedBy = entity.UpdatedByUser?.FullName ?? string.Empty
                    });
                }

                return ApiResult<List<CvEntryHistoryDto>>.Success(cvHistoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return ApiResult<List<CvEntryHistoryDto>>.Failure("An error occurred while retrieving the CV entry.");
            }
        }

        public async Task<ApiResult<CvEntryHistoryDto>> GetHistoryDetailsById(Guid id)
        {
            try
            {
              
                var entity = await _cvEntryHistoryRepository.FirstOrDefaultAsync(
                    e => e.Id == id,
                    ignoreGlobalFilter: true
                );

                if (entity == null)
                {
                    return ApiResult<CvEntryHistoryDto>.Failure("CV entry not found or access denied.");
                }


                var entryData = string.IsNullOrWhiteSpace(entity.EntryData)
                    ? new CvEntryDataDto()
                    : JsonSerializer.Deserialize<CvEntryDataDto>(entity.EntryData, jsonOptions);

                var cvEntryDto = new CvEntryHistoryDto
                {
                    Id = entity.Id,
                    CVEntryId = entity.CVEntryId,
                    IsDeleted = entity.IsDeleted,
                    EntryHistoryData = entryData,
                    CreatedAt = entity.CreatedAt,
                    CreatedBy = entity.CreatedByUser?.FullName ?? string.Empty,
                    UpdatedAt = entity.UpdatedAt,
                    UpdatedBy = entity.UpdatedByUser?.FullName ?? string.Empty,
                };

                return ApiResult<CvEntryHistoryDto>.Success(cvEntryDto);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return ApiResult<CvEntryHistoryDto>.Failure("An error occurred while retrieving the CV history entry.");
            }
        }


        public async Task<ApiResult<bool>> RestoreFromHistory(Guid historyEntryId)
        {
            try
            {
                var historyEntry = await _cvEntryHistoryRepository.FirstOrDefaultAsync(e => e.Id == historyEntryId);
                if (historyEntry == null)
                {
                    return ApiResult<bool>.NotFound("History entry not found.");
                }

                var entryData = JsonSerializer.Deserialize<CvEntryDataDto>(historyEntry.EntryData, jsonOptions);
                if (entryData == null)
                {
                    return ApiResult<bool>.Failure("Failed to deserialize entry data from history.");
                }

                var cvEntry = await _cvEntryRepository.FirstOrDefaultAsync(e => e.Id == historyEntry.CVEntryId);
                if (cvEntry == null)
                {
                    return ApiResult<bool>.NotFound("CV entry not found.");
                }

                cvEntry.EntryData = JsonSerializer.Serialize(entryData, jsonOptions);
                await _cvEntryRepository.UpdateAsync(cvEntry);

                var newHistoryEntry = new CvEntryHistory
                {
                    CVEntryId = cvEntry.Id,
                    EntryData = cvEntry.EntryData,
                };

                await _cvEntryHistoryRepository.InsertAsync(newHistoryEntry);

                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return ApiResult<bool>.Failure("An error occurred while restoring data from history.");
            }
        }

    }

}

