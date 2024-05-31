using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Entities
{
    public class InvoiceDetail : BaseEntityNew<Guid>
    {
        public int InvoiceId { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Price { get; set; }
        public decimal Vat { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsDeleted { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public virtual Invoice Invoices{ get; set; }
    }
}
