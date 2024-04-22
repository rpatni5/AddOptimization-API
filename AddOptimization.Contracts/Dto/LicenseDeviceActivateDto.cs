using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Contracts.Dto
{
    public class LicenseDeviceManagementDto
    {
        [Required]
        public string LicenseKey { get; set; }
        public string MachineName { get; set; }
    }
}