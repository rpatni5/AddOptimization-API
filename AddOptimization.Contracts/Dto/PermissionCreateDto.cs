using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Contracts.Dto;

public class PermissionCreateDto
{
    [Required]
    public Guid RoleId { get; set; }
    public Guid ScreenId { get; set; }
    public List<FieldDto> Fields { get; set; }
}
