using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class InvoiceHistory : BaseEntityNew<Guid>
    {
        public long InvoiceId { get; set; }
        public Guid InvoiceStatusId { get; set; }
        public string Comment { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public virtual Invoice Invoice { get; set; }

        [ForeignKey(nameof(InvoiceStatusId))]
        public virtual InvoiceStatus InvoiceStatus { get; set; }
    }

}
