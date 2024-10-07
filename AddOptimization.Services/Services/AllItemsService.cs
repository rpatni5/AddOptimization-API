using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Services.Services
{
    public class AllItemsService : IAllItemsService
    {
        private readonly ITemplateEntryService _templateEntryService;
        private readonly ILogger<AllItemsService> _logger;

        public AllItemsService(ITemplateEntryService templateEntryService, ILogger<AllItemsService> logger)
        {
            _templateEntryService = templateEntryService;
            _logger = logger;
        }
        public async Task<ApiResult<List<TemplateEntryDto>>> getAllTemplates()
        {
            try
            {
                var mappedEntity = (await _templateEntryService.Search(null)).Result;
                return ApiResult<List<TemplateEntryDto>>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "an error occured while fetching templates");
                throw;
            }
        }
    }
}
