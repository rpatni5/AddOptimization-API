using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Contracts.Dto;

public class PermissionDeleteDto
{
    [Required]
    public Guid RoleId { get; set; }
    [Required]
    public Guid ScreenId { get; set; }
}
