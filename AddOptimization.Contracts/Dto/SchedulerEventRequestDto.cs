namespace AddOptimization.Contracts.Dto
{
    public class SchedulerEventRequestDto 
    {
        public Guid ClientId { get; set; }
        public int? UserId { get; set; }
        public int ApprovarId { get; set; }
        public DateTime DateMonth { get; set; }
       
    }
}
