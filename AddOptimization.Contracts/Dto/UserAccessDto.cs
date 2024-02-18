namespace AddOptimization.Contracts.Dto;

public class UserAccessDto
{
    public Guid? PermissionsVersion { get; set; }
    public List<string> Roles { get; set; }
    public List<ScreenDto> Screens { get; set; }
    public int UnreadNotificationCount { get; set; }
    public bool IsEmailsEnabled { get; set; }
}
