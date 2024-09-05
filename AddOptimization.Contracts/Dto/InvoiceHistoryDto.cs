using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class InvoiceHistoryDto
    {
        public Guid Id { get; set; }
        public long InvoiceId { get; set; }
        public Guid InvoiceStatusId { get; set; }
        public string InvoiceStatusName { get; set; }
        public string Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }

    }
}
