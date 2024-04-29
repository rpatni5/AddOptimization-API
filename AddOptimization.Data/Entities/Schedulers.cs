using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;
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

        [ForeignKey(nameof(StatusID))]
        public virtual SchedulerStatus SchedulerStatus { get; set; }


        [ForeignKey(nameof(EventTypeID))]

        public virtual SchedulerEventType SchedulerEventType { get; set; }


        [ForeignKey(nameof(UserID))]

        public virtual ApplicationUser ApplicationUser { get; set; }

    }

}
