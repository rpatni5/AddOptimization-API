using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class GuiVersionResponseDto : BaseDto<Guid>
    {
        public string GuiVersionNo { get; set; }
        public string FrameworkVersionNo { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public string DownloadPath { get; set; }
        public bool IsLatest { get; set; }  

    }
}


