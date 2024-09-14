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

        public TemplateService(IGenericRepository<Template> templateRepository, ILogger<TemplateService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _templateRepository = templateRepository;
            _logger = logger;
            _mapper = mapper;
        }

        
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

    }

}

