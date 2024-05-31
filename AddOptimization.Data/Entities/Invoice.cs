using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Entities
{
    public class Invoice : BaseEntityNew<int>
    {
        public string InvoiceNumber { get; set; }
        public string InvoiceDate { get; set; }
        public Guid CustomerId { get; set; }
        public Guid PaymentStatusId { get; set; }
        public Guid InvoiceStatusId { get; set; }
        public bool IsDeleted { get; set; }

        [ForeignKey(nameof(PaymentStatusId))]
        public virtual PaymentStatus PaymentStatus { get; set; }

        [ForeignKey(nameof(InvoiceStatusId))]
        public virtual InvoiceStatus InvoiceStatus { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public virtual Customer Customer { get; set; }

        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; }

    }
}
