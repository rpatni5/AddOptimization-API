using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Services.Constants;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace AddOptimization.Services.Services
{
    public class InvoiceCreditNoteService:IInvoiceCreditNoteService
    {
        private readonly IGenericRepository<InvoicePaymentHistory> _invoicePaymentRepository;
        private readonly IGenericRepository<InvoiceCreditNotes> _invoiceCreditNoteRepository;
        private readonly IInvoiceStatusService _invoiceStatusService;
        private readonly ILogger<InvoiceCreditNoteService> _logger;
        private readonly IPaymentStatusService _paymentStatusService;
        private readonly IMapper _mapper;
        private readonly IInvoiceService _invoiceService;
        private readonly IGenericRepository<Invoice> _invoiceRepository;

        public InvoiceCreditNoteService(IGenericRepository<InvoiceCreditNotes> invoiceCreditNoteRepository, IGenericRepository<InvoicePaymentHistory> invoicePaymentRepository, ILogger<InvoiceCreditNoteService> logger, IMapper mapper, IInvoiceStatusService invoiceStatusService, IInvoiceService invoiceService, IPaymentStatusService paymentStatusService, IGenericRepository<Invoice> invoiceRepository)
        {
            _invoiceCreditNoteRepository = invoiceCreditNoteRepository;
            _logger = logger;
            _mapper = mapper;
            _invoiceStatusService = invoiceStatusService;
            _paymentStatusService = paymentStatusService;
            _invoiceService = invoiceService;
            _invoiceRepository = invoiceRepository;
            _invoicePaymentRepository=invoicePaymentRepository;
        }

        public async Task<ApiResult<InvoiceCreditPaymentDto>> Create(InvoiceCreditPaymentDto model)
        {
            try
            {
                var eventStatus = (await _invoiceStatusService.Search()).Result;
                var paymentStatus = (await _paymentStatusService.Search()).Result;
                var closedStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusesEnum.CLOSED.ToString()).Id;
                var existingcreditNote = await _invoiceCreditNoteRepository.QueryAsync(e => e.InvoiceId == model.InvoiceId);
                foreach (var payment in existingcreditNote.ToList())
                {
                    await _invoiceCreditNoteRepository.DeleteAsync(payment);
                }

                var entities = new List<InvoiceCreditNotes>();

                foreach (var summary in model.InvoiceCreditNotes)
                {
                    var newEntity = new InvoiceCreditNotes
                    {
                        Id = Guid.NewGuid(),
                        InvoiceId = model.InvoiceId,
                        PaymentDate = summary.PaymentDate,
                        Amount = summary.Amount,
                        TransactionId = summary.TransactionId,
                    };
                    await _invoiceCreditNoteRepository.InsertAsync(newEntity);
                    entities.Add(newEntity);
                }


                var mappedEntity = _mapper.Map<List<InvoiceCreditNoteDto>>(entities);

                var invoiceAmountPayment = new InvoiceCreditPaymentDto
                {
                    InvoiceId = model.InvoiceId,
                    InvoiceCreditNotes = mappedEntity
                };
                var existingPayments = await _invoicePaymentRepository.QueryAsync(e => e.InvoiceId == model.InvoiceId);
                var paymentEntities = existingPayments.ToList();

                var totalPaidAmount = paymentEntities.Sum(x => x.Amount) +   invoiceAmountPayment.InvoiceCreditNotes.Where(x => !x.IsDeleted).Sum(x => x.Amount);

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
                invoice.HasCreditNotes = entities.Any();

                if (invoice.CreditNoteNumber == null && entities.Any())
                {
                    long maxInvoiceCreditNoteNumber = (await _invoiceRepository.QueryAsync(x => x.CreditNoteNumber != null)).Count();

                    long newInvoiceCreditNoteNumber = long.Parse($"{DateTime.UtcNow:yyyyMM}{(maxInvoiceCreditNoteNumber + 1)}");


                    invoice.CreditNoteNumber = newInvoiceCreditNoteNumber;
                }


                await _invoiceRepository.UpdateAsync(invoice);

                return ApiResult<InvoiceCreditPaymentDto>.Success(invoiceAmountPayment);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        public async Task<ApiResult<List<InvoiceCreditNoteDto>>> GetCreditInfoById(int id)
        {
            try
            {
                var entity = (await _invoiceCreditNoteRepository.QueryAsync(e => e.InvoiceId == id, disableTracking: true)).ToList();
                if (entity == null)
                {
                    return ApiResult<List<InvoiceCreditNoteDto>>.NotFound("CreditNote");
                }
                var mappedEntity = _mapper.Map<List<InvoiceCreditNoteDto>>(entity);

                return ApiResult<List<InvoiceCreditNoteDto>>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
    }
}
