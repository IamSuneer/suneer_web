using System.ComponentModel.DataAnnotations;

namespace suneer_web.Models;

public class Education
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string School { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Degree { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}
