using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Entities;

public class ImageEntity
{
    [Key]
    public Guid Id { get; init; }

    [Required]
    [Column(TypeName = "nvarchar(75)")]
    public string Name { get; init; } = null!;

    // Store the URL path to the Image (Using Azure Blob Storage or similar)
    [Required]
    [Column(TypeName = "nvarchar(250)")]
    public string Path { get; init; } = null!;

    [Column(TypeName = "nvarchar(250)")]
    public string? Description { get; init; }

    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    [Required]
    [Column(TypeName = "nvarchar(250)")]
    public string AltText { get; init; } = null!;
}
