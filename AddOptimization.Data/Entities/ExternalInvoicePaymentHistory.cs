using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Entities
{
    public  class ExternalInvoicePaymentHistory : BaseEntityNew<Guid>
    {
        public long InvoiceId { get; set; }
        public Guid InvoiceStatusId { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public virtual ExternalInvoice ExternalInvoice { get; set; }

        [ForeignKey(nameof(InvoiceStatusId))]
        public virtual InvoiceStatus InvoiceStatus { get; set; }

        [ForeignKey(nameof(InvoiceStatusId))]
        public virtual PaymentStatus PaymentStatus { get; set; }

        public Guid PaymentStatusId { get; set; }

        public DateTime PaymentDate { get; set; }

        public decimal Amount { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsActive { get; set; }
        public long TransactionId { get; set; }
    }
}
