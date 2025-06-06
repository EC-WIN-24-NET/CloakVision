using System.ComponentModel.DataAnnotations;

namespace Core.DTOs;

public class ImageCreate
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    // This path should be the intended blob name/path within the container,
    // not a full URL or a local file system path unless handled specifically during upload.
    public string Path { get; set; } = null!;

    public string? Description { get; set; }

    [Required]
    public string AltText { get; set; } = null!;
}
