using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace AddOptimization.Contracts.Dto
{
    public class LicenseDeviceDto
    {
        public Guid CustomerId { get; set; }
        public string MotherBoardId { get; set; }
        public string MachineName { get; set; }
        public Guid LicenseId { get; set; }
    }
}