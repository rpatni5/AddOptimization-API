using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class ExternalInvoiceAmountDto
    {
        public long InvoiceId { get; set; }
        public List<ExternalInvoicePaymentHistoryDto> ExternalInvoicePaymentHistory { get; set; }
    }
}
