using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class SchedulerEvent : BaseEntityNew<Guid>
    {
        public Guid UserStatusId { get; set; }
        public Guid AdminStatusId { get; set; }
        public Guid CustomerId { get; set; }
        public int UserId { get; set; }
        public bool IsDraft { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public int ApprovarId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [ForeignKey(nameof(UserStatusId))]
        public virtual SchedulerStatus UserStatus { get; set; }

        [ForeignKey(nameof(AdminStatusId))]
        public virtual SchedulerStatus AdminStatus { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser ApplicationUser { get; set; }


        [ForeignKey(nameof(CustomerId))]
        public virtual Customer Customer { get; set; }

        [ForeignKey(nameof(ApprovarId))]
        public virtual ApplicationUser Approvar { get; set; }

        public virtual ICollection<SchedulerEventDetails> EventDetails { get; set; }

    }

}
