using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Entities
{
    public class InvoiceDetail : BaseEntityNew<Guid>
    {
        public long InvoiceId { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal VatPercent { get; set; }
        public decimal TotalPriceIncludingVat { get; set; }
        public decimal TotalPriceExcludingVat { get; set; }
        public string Metadata { get; set; }
        public bool IsDeleted { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public virtual Invoice Invoices{ get; set; }
    }
}
