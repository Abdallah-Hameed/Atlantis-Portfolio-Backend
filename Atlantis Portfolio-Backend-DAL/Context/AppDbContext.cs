using Atlantis_Portfolio_Backend_DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atlantis_Portfolio_Backend_DAL.Context;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Entities.Action> Actions { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Education> Educations { get; set; }

    public virtual DbSet<Experience> Experiences { get; set; }

    public virtual DbSet<PortfolioProfile> PortfolioProfiles { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectSkill> ProjectSkills { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<SecuritySetting> SecuritySettings { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<SocialLink> SocialLinks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entities.Action>(entity =>
        {
            entity.HasIndex(e => e.ActionName, "UQ_Actions_Name").IsUnique();

            entity.Property(e => e.ActionId).HasColumnName("ActionID");
            entity.Property(e => e.ActionName).HasMaxLength(50);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId);

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.ActionId).HasColumnName("ActionID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(45)
                .HasColumnName("IPAddress");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Action).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.ActionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuditLogs_Actions");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_AuditLogs_Users");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasIndex(e => e.DisplayOrder, "UX_Courses_DisplayOrder").IsUnique();

            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.CertificateUrl).HasMaxLength(500);
            entity.Property(e => e.CourseName).HasMaxLength(200);
            entity.Property(e => e.Provider).HasMaxLength(150);
        });

        modelBuilder.Entity<Education>(entity =>
        {
            entity.HasIndex(e => e.DisplayOrder, "UX_Educations_DisplayOrder").IsUnique();

            entity.Property(e => e.EducationId).HasColumnName("EducationID");
            entity.Property(e => e.Degree).HasMaxLength(150);
            entity.Property(e => e.FieldOfStudy).HasMaxLength(200);
            entity.Property(e => e.Institution).HasMaxLength(200);
        });

        modelBuilder.Entity<Experience>(entity =>
        {
            entity.HasIndex(e => e.DisplayOrder, "UX_Experiences_DisplayOrder").IsUnique();

            entity.Property(e => e.ExperienceId).HasColumnName("ExperienceID");
            entity.Property(e => e.Company).HasMaxLength(200);
            entity.Property(e => e.Position).HasMaxLength(200);
        });

        modelBuilder.Entity<PortfolioProfile>(entity =>
        {
            entity.ToTable("PortfolioProfile");

            entity.Property(e => e.PortfolioProfileId).HasColumnName("PortfolioProfileID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.ProfessionalTitle).HasMaxLength(200);
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasIndex(e => e.DisplayOrder, "UX_Projects_DisplayOrder").IsUnique();

            entity.ToTable(t => t.HasCheckConstraint("CK_Projects_DisplayOrder", "[DisplayOrder] >= 1"));

            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.GitHubUrl).HasMaxLength(500);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.LiveUrl).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<ProjectSkill>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.SkillId }, "UQ_ProjectSkills_Project_Skill").IsUnique();
            entity.HasIndex(e => e.SkillId, "IX_ProjectSkills_SkillID");

            entity.Property(e => e.ProjectSkillId).HasColumnName("ProjectSkillID");
            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");
            entity.Property(e => e.SkillId).HasColumnName("SkillID");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectSkills)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProjectSkills_Projects");

            entity.HasOne(d => d.Skill).WithMany(p => p.ProjectSkills)
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProjectSkills_Skills");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId)
                .HasName("PK__RefreshT__F5845E394CC40CB9");

            entity.HasIndex(e => e.TokenHash)
                .IsUnique()
                .HasDatabaseName("UQ_RefreshTokens_TokenHash");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.RevokedAt).HasColumnType("datetime");
            entity.Property(e => e.TokenHash).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefreshTokens_User");
        });

        modelBuilder.Entity<SecuritySetting>(entity =>
        {
            entity.Property(e => e.SecuritySettingId).HasColumnName("SecuritySettingID");
            entity.Property(e => e.CriticalActionSecretHash).HasMaxLength(255);
            entity.Property(e => e.LockoutUntil).HasColumnType("datetime");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasIndex(e => e.SkillName, "UQ_Skills_Name").IsUnique();

            entity.HasIndex(e => new { e.SkillCategory, e.DisplayOrder }, "UX_Skills_Category_DisplayOrder").IsUnique();

            entity.Property(e => e.SkillId).HasColumnName("SkillID");
            entity.Property(e => e.SkillCategory).HasMaxLength(100);
            entity.Property(e => e.SkillName).HasMaxLength(100);
        });

        modelBuilder.Entity<SocialLink>(entity =>
        {
            entity.HasIndex(e => e.Url, "UQ_SocialLinks_Url").IsUnique();

            entity.HasIndex(e => e.DisplayOrder, "UX_SocialLinks_DisplayOrder").IsUnique();

            entity.Property(e => e.SocialLinkId).HasColumnName("SocialLinkID");
            entity.Property(e => e.Platform).HasMaxLength(100);
            entity.Property(e => e.Url).HasMaxLength(500);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.LastLoginAt).HasColumnType("datetime");
            entity.Property(e => e.LockoutUntil).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}