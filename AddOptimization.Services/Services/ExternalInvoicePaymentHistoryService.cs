using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Services.Constants;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace AddOptimization.Services.Services
{
    public class ExternalInvoicePaymentHistoryService : IExternalInvoicePaymentHistoryService
    {
        private readonly IGenericRepository<ExternalInvoicePaymentHistory> _externalInvoicePaymentRepository;
        private readonly IExternalInvoiceService _externalInvoiceService;
        private readonly IInvoiceStatusService _invoiceStatusService;
        private readonly IPaymentStatusService _paymentStatusService;
        private readonly ILogger<ExternalInvoicePaymentHistoryService> _logger;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<ExternalInvoice> _externalInvoiceRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<ExternalInvoiceDetail> _invoiceDetailRepository;
        private readonly IGenericRepository<ExternalInvoiceHistory> _externalInvoiceHistoryRepository;
        public ExternalInvoicePaymentHistoryService(IGenericRepository<ExternalInvoicePaymentHistory> externalInvoicePaymentRepository, ILogger<ExternalInvoicePaymentHistoryService> logger, IInvoiceStatusService invoiceStatusService, IGenericRepository<ExternalInvoiceHistory> externalInvoiceHistoryRepository, IPaymentStatusService paymentStatusService, IExternalInvoiceService externalInvoiceService, IGenericRepository<ExternalInvoice> externalInvoiceRepository, IMapper mapper)
        {
            _externalInvoicePaymentRepository = externalInvoicePaymentRepository;
            _invoiceStatusService= invoiceStatusService;
            _paymentStatusService= paymentStatusService;
            _externalInvoiceService = externalInvoiceService;
            _externalInvoiceRepository = externalInvoiceRepository;
            _externalInvoiceHistoryRepository = externalInvoiceHistoryRepository;
            _logger = logger;
            _mapper = mapper;
        }
     
        public async Task<ApiResult<ExternalInvoiceAmountDto>> Create(ExternalInvoiceAmountDto model)
        {
            try
            {

                var eventStatus = (await _invoiceStatusService.Search()).Result;
                var paymentStatus = (await _paymentStatusService.Search()).Result;
                var closedStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusesEnum.CLOSED.ToString()).Id;

                var existingPayments = await _externalInvoicePaymentRepository.QueryAsync(e => e.InvoiceId == model.InvoiceId);
                foreach (var payment in existingPayments.ToList())
                {
                    await _externalInvoicePaymentRepository.DeleteAsync(payment);
                }

                var entities = new List<ExternalInvoicePaymentHistory>();

                foreach (var summary in model.ExternalInvoicePaymentHistory)
                {
                    var newEntity = new ExternalInvoicePaymentHistory
                    {
                        Id = Guid.NewGuid(),
                        InvoiceId = model.InvoiceId,
                        PaymentDate = summary.PaymentDate,
                        Amount = summary.Amount,
                        TransactionId = summary.TransactionId,
                    };
                    await _externalInvoicePaymentRepository.InsertAsync(newEntity);
                    entities.Add(newEntity);
                }


                var mappedEntity = _mapper.Map<List<ExternalInvoicePaymentHistoryDto>>(entities);

                var invoiceAmountPayment = new ExternalInvoiceAmountDto
                {
                    InvoiceId = model.InvoiceId,
                    ExternalInvoicePaymentHistory = mappedEntity
                };

                var totalPaidAmount = invoiceAmountPayment.ExternalInvoicePaymentHistory.Where(x => !x.IsDeleted).Sum(x => x.Amount);
                var invoice = await _externalInvoiceRepository.FirstOrDefaultAsync(x => x.Id == model.InvoiceId);

                Guid paymentStatusId;

                var dueAmount = invoice.TotalPriceIncludingVat;
                if (invoice.TotalPriceIncludingVat == totalPaidAmount)
                {
                    paymentStatusId = paymentStatus.FirstOrDefault(x => x.StatusKey == PaymentStatusesEnum.PAID.ToString()).Id;
                    dueAmount = 0;
                    invoice.InvoiceStatusId = closedStatusId;
                    var invoiceHistory = new ExternalInvoiceHistory
                    {
                        InvoiceId = invoice.Id,
                        InvoiceStatusId = invoice.InvoiceStatusId,
                        Comment = "Invoice Closed"
                    };
                    await _externalInvoiceHistoryRepository.InsertAsync(invoiceHistory);

                }
                else if (invoice.TotalPriceIncludingVat > 0)
                {
                    paymentStatusId = paymentStatus.FirstOrDefault(x => x.StatusKey == PaymentStatusesEnum.PARTIAL_PAID.ToString()).Id;
                    dueAmount = invoice.TotalPriceIncludingVat - totalPaidAmount;
                }
                else
                {
                    paymentStatusId = paymentStatus.FirstOrDefault(x => x.StatusKey == PaymentStatusesEnum.UNPAID.ToString()).Id;
                    dueAmount = invoice.TotalPriceIncludingVat;
                }

                invoice.PaymentStatusId = paymentStatusId;
                invoice.DueAmount = dueAmount;
                await _externalInvoiceRepository.UpdateAsync(invoice);

                return ApiResult<ExternalInvoiceAmountDto>.Success(invoiceAmountPayment);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<List<ExternalInvoicePaymentHistoryDto>>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _externalInvoicePaymentRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(x => x.ExternalInvoice), orderBy: x => x.OrderByDescending(x => x.CreatedAt), ignoreGlobalFilter: true);

                var result = entities.ToList();
                var mappedEntities = _mapper.Map<List<ExternalInvoicePaymentHistoryDto>>(result);

                return ApiResult<List<ExternalInvoicePaymentHistoryDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<ApiResult<List<ExternalInvoicePaymentHistoryDto>>> GetPaymentById(int id)
        {
            try
            {
                var entity = (await _externalInvoicePaymentRepository.QueryAsync(e => e.InvoiceId == id, disableTracking: true)).ToList();
                if (entity == null)
                {
                    return ApiResult<List<ExternalInvoicePaymentHistoryDto>>.NotFound("payment");
                }
                var mappedEntity = _mapper.Map<List<ExternalInvoicePaymentHistoryDto>>(entity);

                return ApiResult<List<ExternalInvoicePaymentHistoryDto>>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
    }

}