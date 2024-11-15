﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class SchedulerEventDetailsDto
    {
        public Guid Id { get; set; }
        public decimal Duration { get; set; }
        public DateTime? Date { get; set; }
        public string Summary { get; set; }
        public Guid EventTypeId { get; set; }
        public int? UserId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid SchedulerEventId { get; set;}
        
    }
}
