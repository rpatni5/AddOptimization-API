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
        private readonly ILogger<CvManagementHistoryService> _logger;
        private readonly IMapper _mapper;

        private readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public CvManagementHistoryService(IGenericRepository<CvEntryHistory> cvEntryHistoryRepository, ILogger<CvManagementHistoryService> logger, IMapper mapper)
        {
            _cvEntryHistoryRepository = cvEntryHistoryRepository;
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
                            .Include(e => e.CvEntry)
                            .Include(e => e.CreatedByUser));

                if (entities == null || !entities.Any())
                {
                    return ApiResult<List<CvEntryHistoryDto>>.NotFound("CV History");
                }

                var latestEntities = entities.Take(5).ToList(); 

                var cvHistoryDtos = new List<CvEntryHistoryDto>();

                foreach (var entity in latestEntities)
                {
                    CvEntryDataDto entryData;
                    try
                    {
                        entryData = string.IsNullOrWhiteSpace(entity.EntryData)
                            ? new CvEntryDataDto()
                            : JsonSerializer.Deserialize<CvEntryDataDto>(entity.EntryData, jsonOptions);
                    }
                    catch (JsonException ex)
                    {
                        entryData = new CvEntryDataDto();
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

    }

}

