using AddOptimization.Data.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Data.Entities
{
    public class Schedulers : BaseEntityNew<Guid>
    {
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

        public virtual SchedulerStatus SchedulerStatus { get; set; }

        public virtual SchedulerEventType SchedulerEventType { get; set; }

    }
    
}
