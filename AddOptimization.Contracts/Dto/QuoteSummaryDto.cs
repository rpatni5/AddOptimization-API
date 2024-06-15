using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Contracts.Dto
{
    public class QuoteSummaryDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Vat { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPriceExcVat { get; set; }
        public decimal TotalPriceIncVat { get; set; }
        public long QuoteId { get; set; }
    }
}
