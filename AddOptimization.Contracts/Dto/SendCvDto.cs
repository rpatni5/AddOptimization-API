using Microsoft.AspNetCore.Http;
using NPOI.HSSF.Record;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class SendCvDto
    {
        public string SendTo { get; set; }
        public string SenderName { get; set; }
        public string ClientName { get; set; }
        public string Sender { get; set; }
        public string UserName { get; set; }
        public Guid CvEntryId { get; set; }
        public string EmployeeName { get; set;}

    }

}
