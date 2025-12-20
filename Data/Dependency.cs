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

    public ICollection<SoftwareDependency> SoftwareUsages { get; set; } = new List<SoftwareDependency>();
}
