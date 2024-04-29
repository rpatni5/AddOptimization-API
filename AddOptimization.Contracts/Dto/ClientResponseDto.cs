using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class ClientResponseDto : BaseDto<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }

        public string ManagerName { get; set; }
        public string ClientEmail { get; set; }
        public Guid? CountryId { get; set; }
        public bool IsApprovalRequired { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }

        public bool IsActive { get; set; }

        public CountryDto Country { get; set; }
    }
}
