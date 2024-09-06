using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
   
    public class QuoteHistoryDto
    {
        public Guid Id { get; set; }
        public long QuoteId { get; set; }
        public Guid QuoteStatusId { get; set; }
        public long QuoteNumber { get; set; } 
        public string QuoteStatusName { get; set; }
        public string Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }

    }
}
