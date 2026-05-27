using System.ComponentModel.DataAnnotations;

namespace suneer_web.Models.ViewModels;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [Display(Name = "Admin Email")]
    public string Email { get; set; } = string.Empty;
}
