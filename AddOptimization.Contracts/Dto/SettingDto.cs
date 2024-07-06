namespace AddOptimization.Contracts.Dto;

public class SettingDto : BaseDto<Guid>
{
    public string Code { get; set; }
    public string Description { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsDeleted { get; set; }

}
