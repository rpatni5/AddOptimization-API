using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class ExternalInvoiceDetailDto
    {
        public Guid Id { get; set; }
        public int ExternalInvoiceId { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal VatPercent { get; set; }
        public decimal TotalPriceIncludingVat { get; set; }
        public decimal TotalPriceExcludingVat { get; set; }
        public string Metadata { get; set; }
        public bool IsDeleted { get; set; }
    }
}
