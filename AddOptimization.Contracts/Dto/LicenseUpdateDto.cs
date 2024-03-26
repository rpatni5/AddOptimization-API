using AddOptimization.Utilities.Enums;

namespace AddOptimization.Contracts.Dto
{
    public class LicenseUpdateDto
    {
        public bool ExpireLicense { get; set; }
        public LicenseDuration LicenseDuration { get; set; }
        public int NoOfDevices { get; set; } = 1;
        public Guid CustomerId { get; set; }
    }
}
