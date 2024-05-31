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
    public class QuoteSummaryService : IQuoteSummaryService
    {
        private readonly IGenericRepository<QuoteSummary> _quoteSummaryRepository;
        private readonly ILogger<QuoteSummaryService> _logger;
        private readonly IMapper _mapper;
        public QuoteSummaryService(IGenericRepository<QuoteSummary> quoteSummaryRepository, ILogger<QuoteSummaryService> logger, IMapper mapper)
        {
            _quoteSummaryRepository = quoteSummaryRepository;
            _logger = logger;
            _mapper = mapper;
        }

        

    }
}