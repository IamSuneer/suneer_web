namespace suneer_web.Models.ViewModels;

public class ResumeViewModel
{
    public List<Experience> Experiences { get; set; } = new();
    public List<Education>  Educations  { get; set; } = new();
    public List<Skill>      Skills      { get; set; } = new();
}
