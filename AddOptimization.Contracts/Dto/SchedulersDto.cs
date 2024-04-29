using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class SchedulersDto
    {
        public Guid Id { get; set; }
        public decimal Duration { get; set; }

        public DateTime? Date { get; set; }

        public string Summary { get; set; }

        public Guid EventTypeID { get; set; }

        public Guid StatusID { get; set; }

        public Guid ClientID { get; set; }

        public int UserID { get; set; }

        public bool IsDraft { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }
}
