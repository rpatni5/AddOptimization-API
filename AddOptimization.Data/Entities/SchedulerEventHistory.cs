using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class SchedulerEventHistory : BaseEntityNew<Guid>
    {
      
        public Guid UserStatusId { get; set; }
        public Guid SchedulerEventId { get; set; }
        public Guid AdminStatusId { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

        [ForeignKey(nameof(UserStatusId))]
        public virtual SchedulerStatus UserStatus { get; set; }

        [ForeignKey(nameof(AdminStatusId))]
        public virtual SchedulerStatus AdminStatus { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser ApplicationUser { get; set; }

        [ForeignKey(nameof(SchedulerEventId))]
        public virtual SchedulerEvent SchedulerEvent { get; set; }
    }

}
