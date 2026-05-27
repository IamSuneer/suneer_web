using System.ComponentModel.DataAnnotations;

namespace suneer_web.Models;

public class Experience
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Company { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Role { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;
}
