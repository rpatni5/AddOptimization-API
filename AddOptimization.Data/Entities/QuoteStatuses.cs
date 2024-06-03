using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class QuoteStatuses 
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string StatusKey { get; set; }

    }

}
