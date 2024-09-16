using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class CreditCardDto
    {
        public string? CardholderName { get; set; }
        public string? CardNumber { get; set; }
        public string? ExpirationDate { get; set; }
        public int? Cvv { get; set; }
        public int? CardPin { get; set; }
        public string? ZipCode { get; set; }
    }
}
