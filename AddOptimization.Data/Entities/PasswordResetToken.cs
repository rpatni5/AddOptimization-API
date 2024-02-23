using AddOptimization.Data.Common;

namespace AddOptimization.Data.Entities;

public class PasswordResetToken:BaseEntityNewCreatedOnlyProps<Guid>
{
    public string Token { get; set; }
    public bool IsExpired { get; set; }
    public DateTime ExpiryDate { get; set; }
}
