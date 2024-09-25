using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class TemplateEntryDto
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public Guid TemplateId { get; set; }
        public Guid? FolderId { get; set; }
        public string Title { get; set; }

        public EntryDataDto EntryData { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }
}
