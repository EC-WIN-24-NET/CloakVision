using Domain;
using Infrastructure.Entities;

namespace Infrastructure.Factories;

/// <summary>
/// Event Factory
/// </summary>
public class ImageFactory : EntityFactoryBase<Image, ImageEntity>
{
    /// <summary>
    /// Creating from Entity object to Domain object
    /// Entity -> Domain
    /// </summary>
    /// <param name="imageEntity"></param>
    /// <returns></returns>
    public override Image ToDomain(ImageEntity imageEntity)
    {
        return new Image
        {
            Id = imageEntity.Id,
            Name = imageEntity.Name,
            Path = imageEntity.Path,
            Description = imageEntity.Description,
            Thumbnail = imageEntity.Thumbnail,
            CreatedAt = imageEntity.CreatedAt,
            AltText = imageEntity.AltText,
        };
    }

    /// <summary>
    /// Creating from Domain object to Entity object
    /// Domain -> Entity
    /// </summary>
    /// <param name="images"></param>
    /// <returns></returns>
    public override ImageEntity ToEntity(Image images)
    {
        return new ImageEntity
        {
            Id = images.Id,
            Name = images.Name,
            Path = images.Path,
            Description = images.Description,
            Thumbnail = images.Thumbnail,
            CreatedAt = images.CreatedAt,
            AltText = images.AltText,
        };
    }
}
