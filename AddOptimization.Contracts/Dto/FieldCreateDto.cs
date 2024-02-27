using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Contracts.Dto;

public class FieldCreateDto
{
    [Required]
    public string Name { get; set; }
    [Required]
    public Guid ScreenId { get; set; }
}
