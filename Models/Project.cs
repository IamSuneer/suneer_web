using System.ComponentModel.DataAnnotations;

namespace suneer_web.Models;

public class Project
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string TechStack { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? GitHubUrl { get; set; }

    [MaxLength(500)]
    public string? LiveUrl { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }
}
