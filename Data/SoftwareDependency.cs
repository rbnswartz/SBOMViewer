using System.ComponentModel.DataAnnotations;

namespace SBOMViewer.Data;

public class SoftwareDependency
{
    public int Id { get; set; }

    public int SoftwareId { get; set; }
    public Software Software { get; set; } = null!;

    public int DependencyId { get; set; }
    public Dependency Dependency { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string Version { get; set; } = string.Empty;

    public DateTime AddedDate { get; set; } = DateTime.UtcNow;
}
