namespace AddOptimization.Contracts.Dto;

public class PermissionDto
{
    public Guid? RoleId { get; set; }
    public string RoleName { get; set; }
    public Guid? ScreenId { get; set; }
    public string Screen { get; set; }
    public List<FieldDto> Fields { get; set; }
}
