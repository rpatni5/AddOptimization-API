﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class LicenseDetailsDto : BaseDto<Guid>
    {
        public Guid Id { get; set; }
        public string LicenseKey { get; set; }

        public int LicenseDuration { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int NoOfDevices { get; set; }

        public int ActivatedDevicesCount { get; set; }
        public int PendingDevicesCount { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }


        public string CustomerEmail { get; set; }
        public string CustomerName { get; set; }
        public List<LicenseDeviceDto> LicenseDevices { get; set; }

    }
}
