using System.ComponentModel.DataAnnotations;

namespace Core.DTOs;

public class ImageDisplay
{
    [Key]
    public Guid Id { get; init; }

    [Required]
    public string Name { get; init; } = null!;

    [Required]
    public string Path { get; init; } = null!;

    public string? Description { get; init; }

    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    [Required]
    public string AltText { get; init; } = null!;
}
