using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Contracts.Dto
{
    public class MicrosoftLoginDto
    {
        [Required]
        public string IdToken { get; set; }
    }
}
