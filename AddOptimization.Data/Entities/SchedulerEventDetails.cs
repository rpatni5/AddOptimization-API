using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class SchedulerEventDetails : BaseEntityNew<Guid>
    {
        public decimal Duration { get; set; }
        public DateTime? Date { get; set; }
        public string Summary { get; set; }
        public Guid EventTypeId { get; set; }
        public int UserId { get; set; }
        public Guid SchedulerEventId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser ApplicationUser { get; set; }

        [ForeignKey(nameof(SchedulerEventId))]
        public virtual SchedulerEvent SchedulerEvent { get; set; }

        [ForeignKey(nameof(EventTypeId))]
        public virtual SchedulerEventType EventTypes { get; set; }

    }
}