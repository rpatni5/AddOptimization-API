using AddOptimization.Data.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AddOptimization.Contracts.Constants;

namespace AddOptimization.Data.Entities
{
    public partial class Client : BaseEntityNew<Guid>
    {

        [MaxLength(200)]
        public string FirstName { get; set; }

        [MaxLength(200)]
        public string LastName { get; set; }

        [MaxLength(2000)]
        public string Organization { get; set; }

        [MaxLength(200)]
        public string ManagerName { get; set; }

        [MaxLength(200)]
        public string ClientEmail { get; set; }

        public Guid? CountryId { get; set; }

        public bool IsApprovalRequired { get; set; }

        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        [ForeignKey(nameof(CountryId))]
        public virtual Country Country { get; set; }



    }
}



  
    