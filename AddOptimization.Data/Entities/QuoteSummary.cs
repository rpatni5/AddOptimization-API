using AddOptimization.Data.Common;
using Stripe;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class QuoteSummary
    {
        public Guid Id { get; set; }
        public long QuoteId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Vat { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPriceExcVat { get; set; }
        public decimal TotalPriceIncVat { get; set; }
        public string? Description { get; set; }

        [ForeignKey(nameof(QuoteId))]
        public virtual Quote Quote { get; set; }

    }

}
