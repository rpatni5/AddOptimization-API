using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Data.Repositories;
using AddOptimization.Services.Constants;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace AddOptimization.Services.Services
{
    public class InvoicePaymentHistoryService : IInvoicePaymentHistoryService
    {
        private readonly IGenericRepository<InvoicePaymentHistory> _invoicePaymentRepository;
        private readonly IInvoiceStatusService _invoiceStatusService;
        private readonly ILogger<InvoicePaymentHistoryService> _logger;
        private readonly IPaymentStatusService _paymentStatusService;
        private readonly IMapper _mapper;
        private readonly IInvoiceService _invoiceService;
        public InvoicePaymentHistoryService(IGenericRepository<InvoicePaymentHistory> invoicePaymentRepository, ILogger<InvoicePaymentHistoryService> logger, IMapper mapper, IInvoiceStatusService invoiceStatusService, IInvoiceService invoiceService, IPaymentStatusService paymentStatusService)
        {
            _invoicePaymentRepository = invoicePaymentRepository;
            _logger = logger;
            _mapper = mapper;
            _invoiceStatusService = invoiceStatusService;
            _paymentStatusService = paymentStatusService;
            _invoiceService = invoiceService;
        }
        public async Task<ApiResult<List<InvoicePaymentHistoryDto>>> Create(List<InvoicePaymentHistoryDto> models)
        {
            try
            {
                var eventStatus = (await _invoiceStatusService.Search()).Result;
                var statusId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusesEnum.DRAFT.ToString()).Id;

                var paymentStatus = (await _paymentStatusService.Search()).Result;
                var paymentStatusId = paymentStatus.FirstOrDefault(x => x.StatusKey == PaymentStatusesEnum.UNPAID.ToString()).Id;
                

                var entities = new List<InvoicePaymentHistory>();
                foreach (var model in models)
                {
                    var entity = new InvoicePaymentHistory
                    {
                        Id = new Guid(),
                        InvoiceId = model.InvoiceId,
                        PaymentDate = model.PaymentDate,
                        PaymentStatusId = paymentStatusId,
                        InvoiceStatusId = statusId,
                        Amount = model.Amount,
                        TransactionId = model.TransactionId
                    };
                    await _invoicePaymentRepository.InsertAsync(entity);
                }

                var mappedEntity = _mapper.Map<List<InvoicePaymentHistoryDto>>(entities);
                return ApiResult<List<InvoicePaymentHistoryDto>>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<List<InvoicePaymentHistoryDto>>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _invoicePaymentRepository.QueryAsync((e => !e.IsDeleted ), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(x => x.PaymentStatus).Include(x => x.InvoiceStatus), orderBy: x => x.OrderByDescending(x => x.CreatedAt), ignoreGlobalFilter: true);

                var result = entities.ToList();
                var mappedEntities = _mapper.Map<List<InvoicePaymentHistoryDto>>(result);

                return ApiResult<List<InvoicePaymentHistoryDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<ApiResult<List<InvoicePaymentHistoryDto>>> GetPaymentById(int id)
        {
            try
            {
                var entity = (await _invoicePaymentRepository.QueryAsync(e => e.InvoiceId == id, disableTracking: true)).ToList();
                if (entity == null)
                {
                    return ApiResult<List<InvoicePaymentHistoryDto>>.NotFound("payment");
                }
                var mappedEntity = _mapper.Map<List<InvoicePaymentHistoryDto>>(entity);

                return ApiResult<List<InvoicePaymentHistoryDto>>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

    }

}
