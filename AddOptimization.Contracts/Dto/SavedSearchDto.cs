using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class SavedSearchDto :BaseDto<Guid>
    {
        [Required]
        [MaxLength(50)]
        public string SearchScreen { get; set; }
        [Required]
        public string SearchData { get; set; }
        public bool? IsDefault { get; set; }
     
    }
}
