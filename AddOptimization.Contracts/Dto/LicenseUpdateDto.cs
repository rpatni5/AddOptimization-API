using AddOptimization.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class LicenseUpdateDto : BaseDto<Guid>
    {
        public bool ExpireLicense { get; set; }
        public LicenseDuration LicenseDuration { get; set; }
        public int NoOfDevices { get; set; } = 1;
        public Guid CustomerId { get; set; }
    }
}
