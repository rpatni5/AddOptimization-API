using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class GuiVersionCreateDto : BaseDto<Guid>
    {
        public string GuiVersionNo { get; set; }
        public string FrameworkVersionNo { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        [Required]
        public IFormFile VersionFile { get; set; }

        public bool IsLatest { get; set; }

    }
}


