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
    public class TemplateFolderService : ITemplateFolderService
    {
        private readonly IGenericRepository<TemplateFolder> _folderRepository;
        private readonly ILogger<TemplateFolderService> _logger;
        private readonly IMapper _mapper;

        public TemplateFolderService(IGenericRepository<TemplateFolder> folderRepository, ILogger<TemplateFolderService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _folderRepository = folderRepository;
            _logger = logger;
            _mapper = mapper;
        }

        
        public async Task<ApiResult<List<TemplateFolderDto>>> GetAllTemplateFolders()
        {
            try
            {
                var entities = await _folderRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser), orderBy: x => x.OrderBy(x => x.CreatedAt));
                var mappedEntities = _mapper.Map<List<TemplateFolderDto>>(entities);
                return ApiResult<List<TemplateFolderDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

    }

}

