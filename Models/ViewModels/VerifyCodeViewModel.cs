using System.ComponentModel.DataAnnotations;

namespace suneer_web.Models.ViewModels;

public class VerifyCodeViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Code is required.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be exactly 6 digits.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must be 6 numeric digits.")]
    [Display(Name = "Reset Code")]
    public string Code { get; set; } = string.Empty;
}
