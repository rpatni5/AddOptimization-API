using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Entities
{
    public class CustomerEmployeeAssociation : BaseEntityNew<Guid>
    { 
        public Guid CustomerId { get; set; }
        public int EmployeeId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public int ApproverId { get; set; }
        public Guid? PublicHolidayCountryId { get; set; }
        public decimal DailyWeightage { get; set; }
        public decimal Overtime { get; set; }
        public decimal PublicHoliday { get; set; }
        public decimal Saturday { get; set; }
        public decimal Sunday { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public virtual ApplicationUser ApplicationUser { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public virtual Customer Customer { get; set; }

        [ForeignKey(nameof(ApproverId))]
        public virtual ApplicationUser Approver { get; set; }

        public ICollection<EmployeeContract> Contracts { get; set; }

        [ForeignKey(nameof(PublicHolidayCountryId))]
        public virtual Country Country { get; set; }
    }
}
