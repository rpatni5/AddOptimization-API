using AddOptimization.Data.Common;

namespace AddOptimization.Data.Entities
{
    public class InvoicingPaymentMode : BaseEntityNew<Guid>
    {
        public string Name { get; set; }
        public string? ModeKey { get; set; }
    }
}
