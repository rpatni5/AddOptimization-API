using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class LicenseDetailsDto
    {
        public Guid Id { get; set; }
        public string LicenseKey { get; set; }

        public int LicenseDuration { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int NoOfDevices { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string CustomerEmail { get; set; }
        public string CustomerName { get; set; }
        public List<LicenseDeviceDto> LicenseDevices { get; set; }

    }
}
