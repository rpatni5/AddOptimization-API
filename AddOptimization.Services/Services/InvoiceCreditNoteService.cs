using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Services.Constants;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AddOptimization.Services.Services
{
    public class InvoiceCreditNoteService:IInvoiceCreditNoteService
    {
        private readonly IGenericRepository<InvoiceCreditNote> _invoiceCreditNoteRepository;
        private readonly IInvoiceStatusService _invoiceStatusService;
        private readonly ILogger<InvoiceCreditNoteService> _logger;
        private readonly IPaymentStatusService _paymentStatusService;
        private readonly IMapper _mapper;
        private readonly IInvoiceService _invoiceService;
        private readonly IGenericRepository<Invoice> _invoiceRepository;

        public InvoiceCreditNoteService(IGenericRepository<InvoiceCreditNote> invoiceCreditNoteRepository, ILogger<InvoiceCreditNoteService> logger, IMapper mapper, IInvoiceStatusService invoiceStatusService, IInvoiceService invoiceService, IPaymentStatusService paymentStatusService, IGenericRepository<Invoice> invoiceRepository)
        {
            _invoiceCreditNoteRepository = invoiceCreditNoteRepository;
            _logger = logger;
            _mapper = mapper;
            _invoiceStatusService = invoiceStatusService;
            _paymentStatusService = paymentStatusService;
            _invoiceService = invoiceService;
            _invoiceRepository = invoiceRepository;
        }

        public async Task<ApiResult<InvoiceCreditPaymentDto>> Create(InvoiceCreditPaymentDto model)
        {
            try
            {


                var eventStatus = (await _invoiceStatusService.Search()).Result;
                var paymentStatus = (await _paymentStatusService.Search()).Result;
                var closedStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusesEnum.CLOSED.ToString()).Id;

                var existingPayments = await _invoiceCreditNoteRepository.QueryAsync(e => e.InvoiceId == model.InvoiceId);
                foreach (var payment in existingPayments.ToList())
                {
                    await _invoiceCreditNoteRepository.DeleteAsync(payment);
                }

                var entities = new List<InvoiceCreditNote>();

                foreach (var summary in model.InvoiceCreditNote)
                {
                    var newEntity = new InvoiceCreditNote
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
                    InvoiceCreditNote = mappedEntity
                };

                var totalPaidAmount = invoiceAmountPayment.InvoiceCreditNote.Where(x => !x.IsDeleted).Sum(x => x.Amount);

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
                    return ApiResult<List<InvoiceCreditNoteDto>>.NotFound("creditNote");
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
