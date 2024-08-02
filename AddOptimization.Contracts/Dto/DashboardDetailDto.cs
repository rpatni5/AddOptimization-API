using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Contracts.Dto
{
    public class DashboardDetailDto 
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public decimal Amount { get; set; }
        public long NoOfInvoice { get; set; }
        public string Type { get; set; }
       
    }
}
