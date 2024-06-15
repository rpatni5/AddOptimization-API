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
    public class InvoiceStatusService : IInvoiceStatusService
    {
        private readonly IGenericRepository<InvoiceStatus> _invoiceStatusRepository;

        private readonly ILogger<InvoiceStatusService> _logger;
        private readonly IMapper _mapper;
        public InvoiceStatusService(IGenericRepository<InvoiceStatus> invoiceStatusRepository, ILogger<InvoiceStatusService> logger, IMapper mapper)
        {
            _invoiceStatusRepository = invoiceStatusRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResult<List<InvoiceStatusDto>>> Search()
        {
            try
            {
                var entities = await _invoiceStatusRepository.QueryAsync();
                var mappedEntities = _mapper.Map<List<InvoiceStatusDto>>(entities.ToList());
                return ApiResult<List<InvoiceStatusDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
    }
}