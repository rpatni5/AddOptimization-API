using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class CvEntryHistoryDto
    {
        public Guid Id { get; set; }
        public Guid CVEntryId { get; set; }
        public string EntryData { get; set; }
        public bool IsDeleted { get; set; }
        public CvEntryDataDto EntryHistoryData { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }

    }
}
