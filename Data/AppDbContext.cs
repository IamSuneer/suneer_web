using Microsoft.EntityFrameworkCore;
using suneer_web.Models;

namespace suneer_web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Experience> Experiences => Set<Experience>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enforce unique email on AdminUser
        modelBuilder.Entity<AdminUser>()
            .HasIndex(a => a.Email)
            .IsUnique();

        // Enforce unique email on PasswordResetToken — one active code per email
        modelBuilder.Entity<PasswordResetToken>()
            .HasIndex(p => p.Email);

        // Skill level is stored as a tinyint-equivalent — constraint enforced in model via [Range]
        modelBuilder.Entity<Skill>()
            .Property(s => s.Level)
            .HasDefaultValue(0);

        // ContactMessage: CreatedAt defaults to UTC now at DB level
        modelBuilder.Entity<ContactMessage>()
            .Property(c => c.CreatedAt)
            .HasDefaultValueSql("datetime('now')");
    }
}
