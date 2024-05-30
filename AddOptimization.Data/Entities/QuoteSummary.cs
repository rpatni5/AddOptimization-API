using AddOptimization.Data.Common;
using Stripe;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class QuoteSummary
    {
        public Guid Id { get; set; }
        public Guid QuoteId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int Vat { get; set; }
        public decimal UnitPrice { get; set; }
        public int TotalPriceExcVat { get; set; }
        public int TotalPriceIncVat { get; set; }

        [ForeignKey(nameof(QuoteId))]
        public virtual Quote Quote { get; set; }

    }

}
