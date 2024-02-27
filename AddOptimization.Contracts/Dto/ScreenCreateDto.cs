using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Contracts.Dto;

public class ScreenCreateDto:BaseDto<Guid?>
{
    [Required]
    public string Route { get; set; }
}
