using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Contracts.Dto
{
    public class CustomerTimesheetActionDto :BaseDto<Guid>
    {
        public bool IsApproved { get; set; }
        public string Comment { get; set; }
    }
}
