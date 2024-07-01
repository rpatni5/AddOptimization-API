using Stripe;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class ExternalInvoicePaymentHistoryDto
    {
        public Guid? Id { get; set; }
        public long InvoiceId { get; set; }
        public Guid InvoiceStatusId { get; set; }
        public Guid PaymentStatusId { get; set; }

        public DateTime PaymentDate { get; set; }

        public decimal Amount { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsActive { get; set; }
        public long TransactionId { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
