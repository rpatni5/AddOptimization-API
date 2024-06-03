using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AutoMapper;
using Microsoft.Extensions.Logging;


namespace AddOptimization.Services.Services
{
    public class QuoteStatusService : IQuoteStatusService
    {
        private readonly IGenericRepository<QuoteStatuses> _quoteStatusRepository;
        private readonly ILogger<QuoteStatusService> _logger;
        private readonly IMapper _mapper;
        public QuoteStatusService(IGenericRepository<QuoteStatuses> quoteStatusRepository, ILogger<QuoteStatusService> logger, IMapper mapper)
        {
            _quoteStatusRepository = quoteStatusRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResult<List<QuoteStatusDto>>> Search()
        {
            try
            {
                var entities = await _quoteStatusRepository.QueryAsync();
                var mappedEntities = _mapper.Map<List<QuoteStatusDto>>(entities.ToList());
                return ApiResult<List<QuoteStatusDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
    }
}