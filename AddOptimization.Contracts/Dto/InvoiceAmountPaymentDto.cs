using Stripe;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class InvoiceAmountPaymentDto

    {
        public long InvoiceId { get; set; }
        public List<InvoicePaymentHistoryDto> InvoicePaymentHistory { get; set; }
    }
}
