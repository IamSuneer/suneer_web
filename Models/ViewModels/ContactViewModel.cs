using System.ComponentModel.DataAnnotations;

namespace suneer_web.Models.ViewModels;

public class ContactViewModel
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100)]
    [Display(Name = "Your Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [MaxLength(200)]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required.")]
    [MinLength(10, ErrorMessage = "Message must be at least 10 characters.")]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;
}
