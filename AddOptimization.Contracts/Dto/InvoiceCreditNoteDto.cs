using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class InvoiceCreditNoteDto
    {
        public Guid? Id { get; set; }
        public long InvoiceId { get; set; }
        public DateTime PaymentDate { get; set; }
        public bool IsDeleted { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
