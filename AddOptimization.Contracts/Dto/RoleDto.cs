namespace AddOptimization.Contracts.Dto;

public class RoleDto : RoleCreateDto
{
    public int UserCount { get; set; }
    public string DepartmentName { get; set; }
}
