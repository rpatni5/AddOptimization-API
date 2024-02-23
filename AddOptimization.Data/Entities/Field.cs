using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Data.Entities;

public class Field
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public Guid? ScreenId { get; set; }
    [ForeignKey(nameof(ScreenId))]
    public Screen Screen { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    [MaxLength(200)]
    public string FieldKey { get; set; }

}
