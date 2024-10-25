using AddOptimization.Contracts.Dto;
using AddOptimization.Data.Common;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class CvEntryHistory : BaseEntityNew<Guid>
    {
        public Guid CVEntryId { get; set; }
        public string EntryData { get; set; }
        public bool IsDeleted { get; set; }

        [ForeignKey(nameof(CVEntryId))]
        public virtual CvEntry CvEntry { get; set; }

    }

}