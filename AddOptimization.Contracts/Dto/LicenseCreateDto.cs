using AddOptimization.Utilities.Enums;
using System.ComponentModel.DataAnnotations;
namespace AddOptimization.Contracts.Dto
{
    public class LicenseCreateDto
    {
        [Required]
        public Guid CustomerId { get; set; }
        public LicenseDuration LicenseDuration { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Number of devices cannot be {1}")]
        public int NoOfDevices { get; set; } = 2;
    }
}
