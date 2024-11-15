﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class CvEntryDto
    {
        public Guid Id { get; set; }
        [Required]
        public int UserId { get; set; }
        public string Title { get; set; }
        public bool IsDeleted { get; set; }
        public CvEntryDataDto EntryData { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public string EmployeeName { get; set; }

    }
}
