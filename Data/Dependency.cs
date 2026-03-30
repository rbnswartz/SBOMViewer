using System.ComponentModel.DataAnnotations;

namespace SBOMViewer.Data;

public class Dependency
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Type { get; set; }

    [StringLength(100)]
    public string? Ecosystem { get; set; }

    [StringLength(1000)]
    public string? PackageUrl { get; set; }

    public ICollection<SoftwareDependency> SoftwareUsages { get; set; } = new List<SoftwareDependency>();

    public ICollection<DependencyVulnerability> Vulnerabilities { get; set; } = new List<DependencyVulnerability>();
}
