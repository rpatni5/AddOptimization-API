using AddOptimization.Data.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Data.Entities
{
    public class SavedSearch : BaseEntityNew<Guid>
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [Required]
        [MaxLength(50)]
        public string SearchScreen { get; set; }
        [Required]
        public string SearchData { get; set; }
        public bool? IsDefault { get; set; }
    }
}
