namespace AddOptimization.Contracts.Dto;

public class ApplicationUserDto :UserSummaryDto
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
    public bool? IsLocked { get; set; }
    public List<RoleDto> Roles { get; set; }
}
