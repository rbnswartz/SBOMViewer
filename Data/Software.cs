using System.ComponentModel.DataAnnotations;

namespace SBOMViewer.Data;

public class Software
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public ICollection<SoftwareDependency> Dependencies { get; set; } = new List<SoftwareDependency>();
}
