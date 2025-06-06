using Core.DTOs;
using Domain;

namespace Core.Interfaces;

public interface IImageService
{
    Task<RepositoryResult<IEnumerable<ImageDisplay>>> GetAllImagesAsync();
    Task<RepositoryResult<ImageDisplay>> GetImageByGuid(Guid id);
    Task<RepositoryResult<IEnumerable<ImageDisplay>>> GetImagesByIdsAsync(
        IEnumerable<Guid> imageIds
    );
    Task<RepositoryResult<ImageDisplay>> CreateImageAsync(ImageCreate imageCreateDto); // Add this line
}
