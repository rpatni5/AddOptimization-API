using AddOptimization.Data.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Data.Entities
{
    public class Notification : BaseEntityNewCreatedOnlyProps<int>
    {
        [Required]
        [MaxLength(200)]
        public string Subject { get; set; }

        public string Content { get; set; }

        [MaxLength(300)]
        public string Link { get; set; }
        public string GroupKey { get; set; }
        public string Meta { get; set; }

        public bool? IsRead { get; set; }

        public int? AppplicationUserId { get; set; }

        public DateTime? ReadAt { get; set; }

        [ForeignKey("AppplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }
    }
}
