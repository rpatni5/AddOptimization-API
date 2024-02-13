using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Entities;

public partial class User : BaseEntity<int>
{
    [MaxLength(200)]
    public string FullName { get; set; }

    [MaxLength(200)]
    public string ProviderId { get; set; }
    public DateTime? LastAction { get; set; }

}
