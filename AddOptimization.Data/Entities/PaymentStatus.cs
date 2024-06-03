using AddOptimization.Data.Common;

namespace AddOptimization.Data.Entities
{
    public class PaymentStatus : BaseEntityNew<Guid>
    {
        public string Name { get; set; }
        public string StatusKey { get; set; }
    }
}
