using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Contracts.Dto
{
    public class SchedulerEventHistoryDto :BaseDto<Guid>
    {
        public int ApprovarId { get; set; }
        public Guid SchedulerEventId { get; set; }
        public string ApprovarName { get; set; }
        public Guid AdminStatusId { get; set; }
        public string AdminStatusName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public string Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
