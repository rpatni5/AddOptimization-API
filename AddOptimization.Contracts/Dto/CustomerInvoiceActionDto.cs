using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Contracts.Dto
{
    public class CustomerInvoiceActionDto :BaseDto<Guid>
    {
        public string Comment { get; set; }
    }
}
