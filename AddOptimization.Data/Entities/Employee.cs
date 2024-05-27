using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class Employee : BaseEntityNew<Guid>
    {
        public int UserId { get; set; }

        public bool IsExternal { get; set; }
        public decimal Salary { get; set; }

        [MaxLength(200)]
        public string BankName { get; set; }

        [MaxLength(200)]
        public string BankAccountName { get; set; }

        [MaxLength(200)]
        public string BankAccountNumber { get; set; }

        [MaxLength(500)]
        public string BillingAddress { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser ApplicationUser { get; set; }

    }

}
