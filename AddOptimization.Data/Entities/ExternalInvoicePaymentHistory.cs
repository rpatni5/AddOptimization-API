using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Entities
{
    public  class ExternalInvoicePaymentHistory : BaseEntityNew<Guid>
    {
        public long InvoiceId { get; set; }
        public DateTime PaymentDate { get; set; }
        public bool IsDeleted { get; set; }
        public decimal Amount { get; set; }
        public long TransactionId { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public virtual ExternalInvoice ExternalInvoice { get; set; }
    }
}