namespace AddOptimization.Contracts.Dto;

public class FieldDto : BaseDto<Guid?>
{
    public Guid? ScreenId { get; set; }
    public string FieldKey { get; set; }
}
