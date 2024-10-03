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
        public  CreditCardDto CreditCardInfo { get; set; }
        public  SecureNoteDto SecureNoteInfo { get; set; }
        public PersonalInformationDto PersonalInfo { get; set; }
        public string Notes { get; set; } 
        public PasswordDto PasswordInfo { get; set; }
        public List<CustomFieldDto> CustomFields { get; set; } 

        public bool IsValueEncrypted { get; set; }
    }
}
