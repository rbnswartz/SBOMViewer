using Microsoft.EntityFrameworkCore;

namespace SBOMViewer.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Software> Software { get; set; } = null!;
    public DbSet<Dependency> Dependencies { get; set; } = null!;
    public DbSet<SoftwareDependency> SoftwareDependencies { get; set; } = null!;
    public DbSet<Vulnerability> Vulnerabilities { get; set; } = null!;
    public DbSet<DependencyVulnerability> DependencyVulnerabilities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure many-to-many relationship
        modelBuilder.Entity<SoftwareDependency>()
            .HasOne(sd => sd.Software)
            .WithMany(s => s.Dependencies)
            .HasForeignKey(sd => sd.SoftwareId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SoftwareDependency>()
            .HasOne(sd => sd.Dependency)
            .WithMany(d => d.SoftwareUsages)
            .HasForeignKey(sd => sd.DependencyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Create indexes for better query performance
        modelBuilder.Entity<Software>()
            .HasIndex(s => s.Name);

        modelBuilder.Entity<Dependency>()
            .HasIndex(d => d.Name);

        modelBuilder.Entity<SoftwareDependency>()
            .HasIndex(sd => new { sd.SoftwareId, sd.DependencyId })
            .IsUnique();

        // Vulnerability relationships
        modelBuilder.Entity<DependencyVulnerability>()
            .HasOne(dv => dv.Vulnerability)
            .WithMany(v => v.AffectedDependencies)
            .HasForeignKey(dv => dv.VulnerabilityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DependencyVulnerability>()
            .HasOne(dv => dv.Dependency)
            .WithMany(d => d.Vulnerabilities)
            .HasForeignKey(dv => dv.DependencyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DependencyVulnerability>()
            .HasIndex(dv => new { dv.DependencyId, dv.VulnerabilityId })
            .IsUnique();

        modelBuilder.Entity<Vulnerability>()
            .HasIndex(v => v.CveId)
            .IsUnique();
    }
}
