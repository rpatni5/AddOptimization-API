namespace AddOptimization.Contracts.Dto;

public class ScreenDto : BaseDto<Guid?>
{
    public string ScreenKey { get; set; }
    public string Route { get; set; }
    public List<FieldDto> Fields { get; set; }

}
