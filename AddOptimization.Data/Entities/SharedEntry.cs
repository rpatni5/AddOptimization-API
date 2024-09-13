using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class SharedEntry : BaseEntityNew<Guid>
    {
        public Guid EntryId { get; set; }
        public int SharedByUserId { get; set; }
        public string SharedWithId { get; set; }
        public string SharedWithType { get; set; }
        public string PermissionLevel { get; set; }
        public DateTime SharedDate { get; set; }

        public bool IsDeleted { get; set; }

        [ForeignKey(nameof(SharedByUserId))]
        public virtual ApplicationUser ApplicationUser { get; set; }

        [ForeignKey(nameof(EntryId))]
        public virtual TemplateEntries TemplateEntries { get; set; }

    }

}