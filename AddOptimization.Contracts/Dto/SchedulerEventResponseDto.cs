using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Contracts.Dto
{
    public class SchedulerEventResponseDto :BaseDto<Guid>
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int ApprovarId { get; set; }
        public string ApprovarName { get; set; }
        public Guid UserStatusId { get; set; }
        public string UserStatusName { get; set; }
        public Guid AdminStatusId { get; set; }
        public string AdminStatusName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool IsDraft { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal WorkDuration { get; set; }
        public decimal Overtime { get; set; }
        public decimal Holiday { get; set; }

        public virtual ApplicationUserDto ApplicationUser { get; set; }

        public virtual ApplicationUserDto Approvar { get; set; }
        public virtual CustomerDto Customer { get; set; }
    }
}
