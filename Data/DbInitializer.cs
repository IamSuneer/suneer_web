using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using suneer_web.Models;

namespace suneer_web.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context, IPasswordHasher<AdminUser> hasher)
    {
        context.Database.Migrate();

        SeedProfile(context);
        SeedSkills(context);
        SeedExperiences(context);
        SeedEducations(context);
        SeedProjects(context);
        SeedAdminUser(context, hasher);
    }

    // -------------------------------------------------------------------------
    // Profile
    // -------------------------------------------------------------------------
    private static void SeedProfile(AppDbContext context)
    {
        if (context.Profiles.Any()) return;

        context.Profiles.Add(new Profile
        {
            Name     = "Suneer Ranjitkar",
            Title    = "Full-Stack Developer & Software Engineer",
            Bio      = "Passionate software engineer with experience building modern web applications. " +
                       "Specialising in .NET, React, and cloud-native architectures. " +
                       "I enjoy turning complex problems into simple, elegant solutions.",
            ImageUrl = "/images/profile.jpg"
        });

        context.SaveChanges();
    }

    // -------------------------------------------------------------------------
    // Skills
    // -------------------------------------------------------------------------
    private static void SeedSkills(AppDbContext context)
    {
        if (context.Skills.Any()) return;

        context.Skills.AddRange(
            new Skill { Name = "C# / .NET",        Level = 90 },
            new Skill { Name = "ASP.NET Core MVC",  Level = 88 },
            new Skill { Name = "Entity Framework",  Level = 85 },
            new Skill { Name = "SQL / SQLite",       Level = 80 },
            new Skill { Name = "JavaScript",         Level = 78 },
            new Skill { Name = "React",              Level = 72 },
            new Skill { Name = "Bootstrap 5",        Level = 85 },
            new Skill { Name = "Git / GitHub",       Level = 90 },
            new Skill { Name = "Docker",             Level = 65 },
            new Skill { Name = "Azure",              Level = 60 }
        );

        context.SaveChanges();
    }

    // -------------------------------------------------------------------------
    // Experience
    // -------------------------------------------------------------------------
    private static void SeedExperiences(AppDbContext context)
    {
        if (context.Experiences.Any()) return;

        context.Experiences.AddRange(
            new Experience
            {
                Company     = "Tech Solutions Ltd.",
                Role        = "Senior Software Engineer",
                StartDate   = new DateTime(2022, 1, 1),
                EndDate     = null,
                Description = "Lead development of internal tooling and client-facing web applications using " +
                              "ASP.NET Core, EF Core, and Azure. Introduced CI/CD pipelines that reduced " +
                              "deployment time by 40%."
            },
            new Experience
            {
                Company     = "Digital Agency Co.",
                Role        = "Full-Stack Developer",
                StartDate   = new DateTime(2019, 6, 1),
                EndDate     = new DateTime(2021, 12, 31),
                Description = "Built and maintained multiple client websites and REST APIs. " +
                              "Worked closely with designers to deliver pixel-perfect UIs with React and Bootstrap."
            },
            new Experience
            {
                Company     = "StartupXYZ",
                Role        = "Junior Developer",
                StartDate   = new DateTime(2017, 3, 1),
                EndDate     = new DateTime(2019, 5, 31),
                Description = "Developed features for a SaaS platform serving 5,000+ users. " +
                              "Gained hands-on experience with MVC architecture and relational databases."
            }
        );

        context.SaveChanges();
    }

    // -------------------------------------------------------------------------
    // Education
    // -------------------------------------------------------------------------
    private static void SeedEducations(AppDbContext context)
    {
        if (context.Educations.Any()) return;

        context.Educations.AddRange(
            new Education
            {
                School    = "Tribhuvan University",
                Degree    = "Bachelor of Science in Computer Science & Information Technology",
                StartDate = new DateTime(2013, 8, 1),
                EndDate   = new DateTime(2017, 6, 30)
            },
            new Education
            {
                School    = "Coursera / Microsoft Learn",
                Degree    = "Azure Developer Associate (AZ-204) — Online Certification",
                StartDate = new DateTime(2021, 1, 1),
                EndDate   = new DateTime(2021, 6, 30)
            }
        );

        context.SaveChanges();
    }

    // -------------------------------------------------------------------------
    // Projects
    // -------------------------------------------------------------------------
    private static void SeedProjects(AppDbContext context)
    {
        if (context.Projects.Any()) return;

        context.Projects.AddRange(
            new Project
            {
                Title       = "CV / Portfolio Website",
                Description = "A professional portfolio and CV site built with ASP.NET Core 8 MVC, " +
                              "EF Core, SQLite, and Bootstrap 5. Features an admin panel for " +
                              "managing all content via a clean UI.",
                TechStack   = "ASP.NET Core 8, EF Core, SQLite, Bootstrap 5, C#",
                GitHubUrl   = "https://github.com/suneer/cv-portfolio",
                LiveUrl     = "https://suneer.dev",
                ImageUrl    = "/images/projects/portfolio.png"
            },
            new Project
            {
                Title       = "Task Management API",
                Description = "A RESTful API for managing tasks and projects, featuring JWT authentication, " +
                              "role-based access control, and real-time updates via SignalR.",
                TechStack   = "ASP.NET Core Web API, JWT, SignalR, PostgreSQL, Docker",
                GitHubUrl   = "https://github.com/suneer/task-api",
                LiveUrl     = null,
                ImageUrl    = "/images/projects/taskapi.png"
            },
            new Project
            {
                Title       = "E-Commerce Dashboard",
                Description = "An admin dashboard for a small e-commerce store with sales analytics, " +
                              "inventory management, and order processing built with React and a .NET backend.",
                TechStack   = "React, TypeScript, ASP.NET Core, EF Core, Chart.js",
                GitHubUrl   = "https://github.com/suneer/ecommerce-dashboard",
                LiveUrl     = "https://demo.suneer.dev/shop",
                ImageUrl    = "/images/projects/ecommerce.png"
            }
        );

        context.SaveChanges();
    }

    // -------------------------------------------------------------------------
    // Admin User
    // Uses ASP.NET Core PasswordHasher<T> (PBKDF2-SHA512, 100k iterations, salted).
    // If a legacy SHA-256 hash is detected on startup it is automatically upgraded.
    // -------------------------------------------------------------------------
    private static void SeedAdminUser(AppDbContext context, IPasswordHasher<AdminUser> hasher)
    {
        const string defaultEmail    = "admin@suneer.dev";
        const string defaultPassword = "Admin@123";

        var existing = context.AdminUsers
            .FirstOrDefault(a => a.Email == defaultEmail);

        if (existing == null)
        {
            // First-run: create with a proper PBKDF2 hash
            var admin = new AdminUser { Email = defaultEmail, PasswordHash = "" };
            admin.PasswordHash = hasher.HashPassword(admin, defaultPassword);
            context.AdminUsers.Add(admin);
            context.SaveChanges();
            return;
        }

        // Upgrade path: Phase 2 stored a 64-char hex SHA-256 digest.
        // PasswordHasher produces a base64 string that is always longer than 64 chars.
        if (existing.PasswordHash.Length == 64 && IsHex(existing.PasswordHash))
        {
            existing.PasswordHash = hasher.HashPassword(existing, defaultPassword);
            context.SaveChanges();
        }
    }

    private static bool IsHex(string value) =>
        value.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
}
