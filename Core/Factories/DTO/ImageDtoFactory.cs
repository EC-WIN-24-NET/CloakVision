using Core.DTOs;
using Core.Interfaces.Factories;
using Domain;

namespace Core.Factories.DTO;

public class ImageDtoFactory : IImageDtoFactory
{
    public ImageDisplay ToDisplay(Image displayImage)
    {
        return new ImageDisplay
        {
            Id = displayImage.Id,
            Name = displayImage.Name,
            Path = displayImage.Path,
            Description = displayImage.Description,
            Thumbnail = displayImage.Thumbnail,
            AltText = displayImage.AltText,
        };
    }

    public Image ToDomain(ImageDisplay imageDisplay)
    {
        return new Image
        {
            Id = imageDisplay.Id,
            Name = imageDisplay.Name,
            Path = imageDisplay.Path,
            Description = imageDisplay.Description,
            Thumbnail = imageDisplay.Thumbnail,
            CreatedAt = imageDisplay.CreatedAt,
            AltText = imageDisplay.AltText,
        };
    }
}
