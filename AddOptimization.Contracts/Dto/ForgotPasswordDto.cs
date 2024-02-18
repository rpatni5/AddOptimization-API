using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Contracts.Dto;

public class ForgotPasswordDto
{
    [Required]
    public string Email { get; set; }
}
public class ResetPasswordDto
{
    public string Password { get; set; }
    public int? UserId { get; set; }
    public string Token { get; set; }
}
