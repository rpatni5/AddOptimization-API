using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class PasswordDto
    {
        public string Email { get; set; }
        public string Password{ get; set; }
        public string WebsiteAddress { get; set; }
        public List<string> Websites { get; set; }
    }
}
