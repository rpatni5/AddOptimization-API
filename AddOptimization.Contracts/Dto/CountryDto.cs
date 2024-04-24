using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class CountryDto : BaseDto<Guid>
    {
     

        public string CountryName { get; set; }

        public string Code { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string CreatedBy { get; set; }


        public DateTime? UpdatedAt { get; set; }

        public string UpdatedBy { get; set; }
    }
}
