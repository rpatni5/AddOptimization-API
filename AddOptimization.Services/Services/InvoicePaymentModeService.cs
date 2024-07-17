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
    public class InvoicePaymentModeService : IInvoicingPaymentModeService
    {
        private readonly IGenericRepository<InvoicingPaymentMode> _invoicePaymentModeRepository;
        private readonly ILogger<InvoicePaymentModeService> _logger;
        private readonly IMapper _mapper;
        public InvoicePaymentModeService(IGenericRepository<InvoicingPaymentMode> invoicePaymentModeRepository, ILogger<InvoicePaymentModeService> logger, IMapper mapper)
        {
            _invoicePaymentModeRepository = invoicePaymentModeRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResult<List<InvoicingPaymentModeDto>>> Search()
        {
            try
            {
                var entities = await _invoicePaymentModeRepository.QueryAsync();
                var mappedEntities = _mapper.Map<List<InvoicingPaymentModeDto>>(entities.ToList());
                return ApiResult<List<InvoicingPaymentModeDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
    }
}