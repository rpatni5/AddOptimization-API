using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class SharedFolderResponseDto
    {
        public Guid Id { get; set; }
        public Guid FolderId { get; set; }
        public int SharedByUserId { get; set; }
        public string SharedWithId { get; set; }
        public string SharedWithType { get; set; }
        public string PermissionLevel { get; set; }
        public DateTime SharedDate { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public string? SharedWithName { get; set; }
        public string SharedTitleName { get; set; }
        public string SharedFolderName { get; set; }
        public Guid TemplateId { get; set; }
        public int? CreatedByUserId { get; set; }
        public List<TemplateEntryDto> TemplateEntries { get; set; }
    }
}
