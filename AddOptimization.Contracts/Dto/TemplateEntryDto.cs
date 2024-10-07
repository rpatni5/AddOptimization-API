using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class TemplateEntryDto
    {
        public Guid Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public Guid TemplateId { get; set; }
        public Guid? FolderId { get; set; }
        public string Title { get; set; }
        public string EntryDataEncrypted { get; set; }

        public EntryDataDto EntryData { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public string Permission {  get; set; }

    }
}
