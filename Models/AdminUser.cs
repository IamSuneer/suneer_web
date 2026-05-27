using System.ComponentModel.DataAnnotations;

namespace suneer_web.Models;

public class AdminUser
{
    public int Id { get; set; }

    [Required, MaxLength(200), EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 hex digest (Phase 2 placeholder).
    /// Phase 3 will replace this with PBKDF2/BCrypt.
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
}
