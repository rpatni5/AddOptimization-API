using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class PublicHolidayDto : BaseDto<Guid>
    {
        public string Title { get; set; }

        public string Info { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string CreatedBy { get; set; }


        public DateTime? UpdatedAt { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? Date { get; set; }

        public bool IsDeleted { get; set; }

        public Guid CountryId { get; set; }


    }
}
