using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class CompanyInformationDto
    {
        public string CompanyName { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactPersonEmail { get; set; }
        public string Country { get; set; }
        public string Address { get; set; }
        public string Address1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }
}
