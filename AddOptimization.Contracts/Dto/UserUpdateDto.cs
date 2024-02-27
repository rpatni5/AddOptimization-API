using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Contracts.Dto;

public class UserUpdateDto
{
    [Required]
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<Guid> Roles { get; set; }
}
