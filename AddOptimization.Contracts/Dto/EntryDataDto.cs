using NPOI.HSSF.Record;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class EntryDataDto
    {
        public string? Title { get; set; }
        public  CreditCardDto? crditCardInfo { get; set; }
        public string? Notes { get; set; }
        public List<CustomFieldDto> CustomFields { get; set; }
    }
}
