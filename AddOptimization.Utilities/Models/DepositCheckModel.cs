using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Utilities.Models
{
    public class DepositCheckModel
    {
        public int OrderId { get; set; }
        public string PayeeName { get; set; }
        public string AccountNumber { get; set; }
        public string CheckNumber { get; set; }
        public long Amount { get; set; }
        public string Comments { get; set; }
    }
}
