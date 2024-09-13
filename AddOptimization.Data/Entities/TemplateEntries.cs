using AddOptimization.Contracts.Dto;
using AddOptimization.Data.Common;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class TemplateEntries : BaseEntityNew<Guid>
    {
        public int UserId { get; set; }
        public Guid TemplateId { get; set; }
        public Guid FolderId { get; set; }
        public string EntryData { get; set; }
        public bool IsDeleted { get; set; }

        [ForeignKey(nameof(TemplateId))]
        public virtual Template Template { get; set; }

        [ForeignKey(nameof(FolderId))]
        public virtual TemplateFolder TemplateFolder { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser ApplicationUser { get; set; }

    }

}