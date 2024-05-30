using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Contracts.Dto
{
    public class QuoteSummaryDto
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int Vat { get; set; }
        public decimal UnitPrice { get; set; }
        public int TotalPriceExcVat { get; set; }
        public int TotalPriceIncVat { get; set; }
        public Guid QuoteId { get; set; }
    }
}
