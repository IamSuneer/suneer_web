using System.ComponentModel.DataAnnotations;

namespace suneer_web.Models;

public class PasswordResetToken
{
    public int Id { get; set; }

    [Required, MaxLength(200), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiryTime { get; set; }

    public bool IsUsed { get; set; } = false;
}
