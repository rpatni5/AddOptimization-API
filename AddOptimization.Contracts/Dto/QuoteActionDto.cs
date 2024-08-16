using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Contracts.Dto
{
    public class QuoteActionDto :BaseDto<long>
    {
        public bool IsApproved { get; set; }
        public string Comment { get; set; }
    }
}
