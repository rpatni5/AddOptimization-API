using AddOptimization.Data.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Data.Entities
{
    public class Product : BaseEntityNew<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public int Quantity { get; set; }
        public decimal ProfitMargin { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
    }
}
