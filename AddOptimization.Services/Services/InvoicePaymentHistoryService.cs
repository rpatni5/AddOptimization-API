using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
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
        private readonly IGenericRepository<Invoice> _invoiceRepository;
        private readonly IGenericRepository<InvoiceCreditNotes> _invoiceCreditNoteRepository;

        public InvoicePaymentHistoryService(IGenericRepository<InvoicePaymentHistory> invoicePaymentRepository, ILogger<InvoicePaymentHistoryService> logger, IMapper mapper, IInvoiceStatusService invoiceStatusService, IInvoiceService invoiceService, IPaymentStatusService paymentStatusService, IGenericRepository<Invoice> invoiceRepository, IGenericRepository<InvoiceCreditNotes> invoiceCreditNoteRepository)
        {
            _invoicePaymentRepository = invoicePaymentRepository;
            _logger = logger;
            _mapper = mapper;
            _invoiceStatusService = invoiceStatusService;
            _paymentStatusService = paymentStatusService;
            _invoiceService = invoiceService;
            _invoiceRepository = invoiceRepository;
            _invoiceCreditNoteRepository= invoiceCreditNoteRepository;
        }

     
        public async Task<ApiResult<InvoiceAmountPaymentDto>> Create(InvoiceAmountPaymentDto model)
        {
            try
            {
                var eventStatus = (await _invoiceStatusService.Search()).Result;
                var paymentStatus = (await _paymentStatusService.Search()).Result;
                var closedStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusesEnum.CLOSED.ToString()).Id;
                var paymentList = (await _invoicePaymentRepository.QueryAsync(e => e.InvoiceId == model.InvoiceId)).ToList();
                foreach (var payment in paymentList)
                {
                    await _invoicePaymentRepository.DeleteAsync(payment);
                }

                var entities = new List<InvoicePaymentHistory>();

                foreach (var summary in model.InvoicePaymentHistory)
                {
                    var newEntity = new InvoicePaymentHistory
                    {
                        Id = Guid.NewGuid(),
                        InvoiceId = model.InvoiceId,
                        PaymentDate = summary.PaymentDate,
                        Amount = summary.Amount,
                        TransactionId = summary.TransactionId,
                    };
                    await _invoicePaymentRepository.InsertAsync(newEntity);
                    entities.Add(newEntity);
                }

                var mappedEntity = _mapper.Map<List<InvoicePaymentHistoryDto>>(entities);

                var invoiceAmountPayment = new InvoiceAmountPaymentDto
                {
                    InvoiceId = model.InvoiceId,
                    InvoicePaymentHistory = mappedEntity
                };
                var existingCreditNotes = await _invoiceCreditNoteRepository.QueryAsync(e => e.InvoiceId == model.InvoiceId);
                var creditNoteEntities = existingCreditNotes.ToList();
                var totalPaidAmount = creditNoteEntities.Sum(x => x.TotalPriceExcludingVat) + invoiceAmountPayment.InvoicePaymentHistory.Where(x => !x.IsDeleted).Sum(x => x.Amount);

                var invoice = await _invoiceRepository.FirstOrDefaultAsync(x => x.Id == model.InvoiceId);

                Guid paymentStatusId;

                var dueAmount = invoice.TotalPriceIncludingVat;
                if (invoice.TotalPriceIncludingVat == totalPaidAmount)
                {
                    paymentStatusId = paymentStatus.FirstOrDefault(x => x.StatusKey == PaymentStatusesEnum.PAID.ToString()).Id;
                    dueAmount = 0;
                    invoice.InvoiceStatusId = closedStatusId;
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
                await _invoiceRepository.UpdateAsync(invoice);

                return ApiResult<InvoiceAmountPaymentDto>.Success(invoiceAmountPayment);
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
                var entities = await _invoicePaymentRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(x => x.Invoice), orderBy: x => x.OrderByDescending(x => x.CreatedAt), ignoreGlobalFilter: true);

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
