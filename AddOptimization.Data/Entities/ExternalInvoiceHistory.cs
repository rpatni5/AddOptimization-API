using AddOptimization.Data.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Data.Entities
{
    public class ExternalInvoiceHistory : BaseEntityNew<Guid>
    {
        public long InvoiceId { get; set; }
        public Guid InvoiceStatusId { get; set; }
        public string Comment { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public virtual ExternalInvoice ExternalInvoice { get; set; }

        [ForeignKey(nameof(InvoiceStatusId))]
        public virtual InvoiceStatus InvoiceStatus { get; set; }
    }
}
