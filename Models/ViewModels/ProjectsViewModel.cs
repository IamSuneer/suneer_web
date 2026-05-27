namespace suneer_web.Models.ViewModels;

public class ProjectsViewModel
{
    public List<Project> Projects { get; set; } = new();
    public List<string>  TechTags { get; set; } = new();
}
