using AddOptimization.Utilities.Enums;
using System;
using System.Collections.Generic;
namespace AddOptimization.Contracts.Dto
{
    public class LicenseCreateDto
    {
        public Guid CustomerId { get; set; }
        public LicenseDuration LicenseDuration { get; set; }
        public int NoOfDevices { get; set; } = 1;
    }
}
