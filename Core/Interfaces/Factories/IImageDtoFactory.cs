using Core.DTOs;
using Domain;

namespace Core.Interfaces.Factories;

public interface IImageDtoFactory
{
    ImageDisplay ToDisplay(Image displayImage);
    Image ToDomain(ImageDisplay imageDisplay);
}
