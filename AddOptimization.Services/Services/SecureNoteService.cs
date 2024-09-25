using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Services.Constants;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;


namespace AddOptimization.Services.Services
{
    public class SecureNoteService : ISecureNoteService
    {
        private readonly IGenericRepository<TemplateEntries> _templateEntryRepository;
        private readonly ILogger<SecureNoteService> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository<ApplicationUser> _applicationUserRepository;
        private readonly IGenericRepository<Group> _groupRepository;
        private readonly ITemplatesService _templateService;
        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public SecureNoteService(IGenericRepository<TemplateEntries> templateEntryRepository, ILogger<SecureNoteService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IGenericRepository<ApplicationUser> applicationUserRepository, IGenericRepository<Group> groupRepository, ITemplatesService templateService)
        {
            _templateEntryRepository = templateEntryRepository;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _applicationUserRepository = applicationUserRepository;
            _groupRepository = groupRepository;
            _templateService = templateService;
        }


        public async Task<ApiResult<bool>> SaveSecureNote(TemplateEntryDto model)
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


        public async Task<ApiResult<List<TemplateEntryDto>>> Search()
        {
            try
            {
                var currentUserId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                var templates = (await _templateService.GetAllTemplate()).Result;
                var secureNoteId = templates.FirstOrDefault(x => x.Name == "Secure Notes".ToString()).Id;
                var entities = await _templateEntryRepository.QueryAsync(
                    e => !e.IsDeleted && e.UserId == currentUserId && e.TemplateId == secureNoteId,
                    include: entities => entities
                        .Include(e => e.CreatedByUser).Include(e => e.TemplateFolder).Include(e => e.Template)
                        .Include(e => e.UpdatedByUser)
                        .Include(e => e.ApplicationUser),
                    orderBy: x => x.OrderByDescending(x => x.CreatedAt)
                );

                var mappedEntities = _mapper.Map<List<TemplateEntryDto>>(entities.ToList());

                return ApiResult<List<TemplateEntryDto>>.Success(mappedEntities);
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
                    return ApiResult<bool>.NotFound("Secure Note");
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


        public async Task<ApiResult<TemplateEntryDto>> GetSecureNoteById(Guid id)
        {
            try
            {
                var entity = (await _templateEntryRepository.QueryAsync(o => o.Id == id && !o.IsDeleted, ignoreGlobalFilter: true)).FirstOrDefault();
                if (entity == null)
                {
                    return ApiResult<TemplateEntryDto>.NotFound("note");
                }
                var mappedEntity = _mapper.Map<TemplateEntryDto>(entity);
                return ApiResult<TemplateEntryDto>.Success(mappedEntity);
            }

            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<ApiResult<TemplateEntryDto>> Update(Guid id, TemplateEntryDto model)
        {
            try
            {
                var entity = await _templateEntryRepository.FirstOrDefaultAsync(e => e.Id == id);
                entity.FolderId = model.FolderId;
                entity.EntryData = System.Text.Json.JsonSerializer.Serialize(model.EntryData, jsonOptions);
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

    }

}

