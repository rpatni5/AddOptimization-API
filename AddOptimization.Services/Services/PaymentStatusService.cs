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
    public class PaymentStatusService : IPaymentStatusService
    {
        private readonly IGenericRepository<PaymentStatus> _paymentStatusRepository;

        private readonly ILogger<PaymentStatusService> _logger;
        private readonly IMapper _mapper;
        public PaymentStatusService(IGenericRepository<PaymentStatus> paymentStatusRepository, ILogger<PaymentStatusService> logger, IMapper mapper)
        {
            _paymentStatusRepository = paymentStatusRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResult<List<PaymentStatusDto>>> Search()
        {
            try
            {
                var entities = await _paymentStatusRepository.QueryAsync();
                var mappedEntities = _mapper.Map<List<PaymentStatusDto>>(entities.ToList());
                return ApiResult<List<PaymentStatusDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
    }
}