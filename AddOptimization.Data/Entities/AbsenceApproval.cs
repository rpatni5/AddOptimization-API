﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AddOptimization.Data.Common;

namespace AddOptimization.Data.Entities
{
    public class AbsenceApproval : BaseEntityNew<Guid>
    {

        [MaxLength(200)]
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public int UserId { get; set; }
        public int LeaveStatusId { get; set; }

        [ForeignKey(nameof(LeaveStatusId))]
        public virtual LeaveStatuses LeaveStatuses { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
