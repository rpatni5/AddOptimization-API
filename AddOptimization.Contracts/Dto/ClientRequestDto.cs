using AddOptimization.Contracts.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class ClientRequestDto : BaseDto<Guid>
    {
        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string ManagerName { get; set; }
        public string ClientEmail { get; set; }
        public Guid? CountryId { get; set; }
        public bool IsApprovalRequired { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }




    }
}
