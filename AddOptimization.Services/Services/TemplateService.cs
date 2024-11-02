using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AddOptimization.Services.Services
{
    public class TemplateService : ITemplatesService
    {
        private readonly IGenericRepository<Template> _templateRepository;
        private readonly ILogger<TemplateService> _logger;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<TemplateEntries> _templateEntryRepository;

        public TemplateService(IGenericRepository<Template> templateRepository, ILogger<TemplateService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IGenericRepository<TemplateEntries> templateEntryRepository)
        {
            _templateRepository = templateRepository;
            _logger = logger;
            _mapper = mapper;
            _templateEntryRepository = templateEntryRepository;}

        
        public async Task<ApiResult<List<TemplateDto>>> GetAllTemplate()
        {
            try
            {
                var entities = await _templateRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser), orderBy: x => x.OrderBy(x => x.CreatedAt));
                var mappedEntities = _mapper.Map<List<TemplateDto>>(entities);
                return ApiResult<List<TemplateDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        public async Task<ApiResult<TemplateDto>> GetTemplateById(Guid id)
        {
            try
            {
                var templateEntry = await _templateEntryRepository.FirstOrDefaultAsync(e => e.Id == id);
                var entity = await _templateRepository.FirstOrDefaultAsync(e => e.Id == templateEntry.TemplateId, ignoreGlobalFilter: true);
                var model = new TemplateDto() {
                    Id = entity.Id,
                    Name = entity.Name,
                    TemplateKey=entity.TemplateKey
                };
            
                return ApiResult<TemplateDto>.Success(model);
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
                    return ApiResult<bool>.NotFound("Item");
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


    }

}

