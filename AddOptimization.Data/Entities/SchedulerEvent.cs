using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class SchedulerEvent : BaseEntityNew<Guid>
    {
        public decimal Duration { get; set; }

        public DateTime? Date { get; set; }

        public string Summary { get; set; }

        public Guid EventTypeId { get; set; }

        public Guid StatusId { get; set; }

        public Guid ClientId { get; set; }

        public int UserId { get; set; }

        public bool IsDraft { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsActive { get; set; }

        [ForeignKey(nameof(StatusId))]
        public virtual SchedulerStatus SchedulerStatus { get; set; }


        [ForeignKey(nameof(EventTypeId))]

        public virtual SchedulerEventType SchedulerEventType { get; set; }


        [ForeignKey(nameof(UserId))]

        public virtual ApplicationUser ApplicationUser { get; set; }

    }

}
