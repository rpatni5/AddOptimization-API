using AddOptimization.Data.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Data.Entities
{
    public class Country : BaseEntityNew<Guid>
    {
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public bool IsDeleted { get; set; }
    }
}
