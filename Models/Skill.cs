using System.ComponentModel.DataAnnotations;

namespace suneer_web.Models;

public class Skill
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Proficiency level 0–100</summary>
    [Range(0, 100)]
    public int Level { get; set; }
}
