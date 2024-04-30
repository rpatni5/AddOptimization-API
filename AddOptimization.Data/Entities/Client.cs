using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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
        public string Email { get; set; }

        public Guid CountryId { get; set; }

        public bool IsApprovalRequired { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        [ForeignKey(nameof(CountryId))]
        public virtual Country Country { get; set; }



    }
}



  
    