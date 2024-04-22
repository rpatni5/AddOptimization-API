namespace AddOptimization.Contracts.Dto
{
    public class LicenseDeviceDto : BaseDto<Guid>
    {
        public Guid CustomerId { get; set; }
        public string MachineName { get; set; }
        public Guid LicenseId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }
}