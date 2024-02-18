
namespace AddOptimization.Contracts.Dto;

public class BaseDto<TId>
{
    public TId Id { get; set; }
    public string Name { get; set; }
}
