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
        public int Id { get; set; }
        public string LicenseKey { get; set; }
        public bool IsExpired { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int NoOfInstance { get; set; }
        public Guid? CustomerId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public CustomerDto Customers { get; set; }
        public LicenseDeviceDto LicenseDeviceDto { get; set; }

    }
}
