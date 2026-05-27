namespace suneer_web.Models.ViewModels;

public class HomeViewModel
{
    public Profile?        Profile          { get; set; }
    public List<Skill>     FeaturedSkills   { get; set; } = new();
    public List<Project>   FeaturedProjects { get; set; } = new();
}
