using System.ComponentModel.DataAnnotations;

namespace suneer_web.Models;

public class Profile
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Bio { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }
}
