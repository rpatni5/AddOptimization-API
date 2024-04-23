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
    public partial class GuiVersion : BaseEntityNew<Guid>
    {
        [MaxLength(100)]
        public string GuiVersionNo { get; set; }
        [MaxLength(100)]
        public string FrameworkVersionNo { get; set; }

        [MaxLength(100)]
        public string DownloadPath { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

    }
}

