namespace suneer_web.Models.ViewModels;

public class AboutViewModel
{
    public Profile?          Profile     { get; set; }
    public List<Experience>  Experiences { get; set; } = new();
    public List<Education>   Educations  { get; set; } = new();
}
