using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Contracts.Dto
{
    public class UserCreateDto
    {
        [Required]
        public string Email { get; set; }
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
