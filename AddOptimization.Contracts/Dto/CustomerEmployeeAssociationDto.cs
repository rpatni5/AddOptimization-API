using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class CustomerEmployeeAssociationDto : BaseDto<Guid?>
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ManagerName { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int ApproverId { get; set; }
        public string ApproverName { get; set; }
        public decimal DailyWeightage { get; set; }
        public decimal Overtime { get; set; }
        public decimal PublicHoliday { get; set; }
        public decimal Saturday { get; set; }
        public decimal Sunday { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public bool? HasContract { get; set; }
        public bool? isExternal { get; set; }



    }
}
