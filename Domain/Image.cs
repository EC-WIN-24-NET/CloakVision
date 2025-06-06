namespace Domain;

public class Image
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Path { get; init; } = null!;
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string AltText { get; init; } = null!;
}
