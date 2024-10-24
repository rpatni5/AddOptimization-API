using AddOptimization.Contracts.Dto;
using AddOptimization.Data.Common;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class CvEntry : BaseEntityNew<Guid>
    {
        public int UserId { get; set; }
        public string EntryData { get; set; }
        public string Title { get; set; }
        public bool IsDeleted { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser ApplicationUser { get; set; }

    }

}