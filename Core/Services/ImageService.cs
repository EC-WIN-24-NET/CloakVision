using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Data;
using Core.Interfaces.Factories;
using Domain;

namespace Core.Services;

public class ImageService(
    IImageRepository imageRepository,
    IRepositoryResultFactory resultFactory,
    IImageDtoFactory imageDtoFactory
) : IImageService
{
    /// <summary>
    /// Retrieves all images from the repository and converts them to display DTOs.
    /// /// Because we are filtering out null images, we can use the `e => e != null` predicate.
    /// and the rest of error handling is done in the repository.
    /// </summary>
    /// <returns></returns>
    public async Task<RepositoryResult<IEnumerable<ImageDisplay>>> GetAllImagesAsync()
    {
        try
        {
            // Get all images from the repository
            var getimages = await imageRepository.GetAllAsync(e => e != null);
            // Check if the repository operation itself failed
            var imageDisplayDTos = getimages
                // Filter out any null images, if necessary
                .Where(e => e != null)
                .Select(e => imageDtoFactory.ToDisplay(e!))
                .ToList();
            // Return the result using the result factory
            return resultFactory.OperationSuccess<IEnumerable<ImageDisplay>>(imageDisplayDTos, 200);
        }
        // Case 1: Repository operation itself failed (e.g., DB connection issue, invalid query).
        catch (Exception ex)
        {
            // Log the exception ex (recommended)
            return resultFactory.OperationFailed<IEnumerable<ImageDisplay>>(
                new Error(
                    "ImageService.GetAll.Exception",
                    $"An unexpected error occurred while retrieving all images: {ex.Message}"
                ),
                500 // Internal Server Error
            );
        }
    }

    public async Task<RepositoryResult<ImageDisplay>> GetImageByGuid(Guid id)
    {
        try
        {
            // Get the images from the repository
            var imageResult = await imageRepository.GetAsync(e => e != null && e.Id == id, false);

            // Case 1: Repository operation itself failed (e.g., DB connection issue, invalid query).
            // imageResult.Error will be a specific error type, not Error.NonError.
            if (imageResult.Error != Error.NonError)
                return resultFactory.OperationFailed<ImageDisplay>(
                    imageResult.Error,
                    imageResult.StatusCode
                );

            // Case 2: Entity was found.
            // BaseRepository.GetAsync returns Value != null and StatusCode = 200.
            // This means the image was successfully retrieved.
            if (imageResult.Value != null)
            {
                var displayimageDto = imageDtoFactory.ToDisplay(imageResult.Value);
                return resultFactory.OperationSuccess(displayimageDto, imageResult.StatusCode);
            }

            // Case 3: Entity was not found.
            if (imageResult.StatusCode == 404)
                return resultFactory.OperationFailed<ImageDisplay>(Error.NotFound("image"), 404);

            // Case 4: Unexpected state after retrieving image details.
            return resultFactory.OperationFailed<ImageDisplay>(
                new Error(
                    "ImageService.UnexpectedState",
                    "An unexpected state was encountered after retrieving image details."
                ),
                500 // Or imageResult.StatusCode if it's a known non-error, non-404 code
            );
        }
        catch (Exception)
        {
            // Log the exception ex (recommended)
            return resultFactory.OperationFailed<ImageDisplay>(
                new Error(
                    "image.RetrievalError",
                    "An unexpected error occurred while retrieving user details."
                ),
                500 // Internal Server Error
            );
        }
    }

    public Task<RepositoryResult<IEnumerable<ImageDisplay>>> GetImagesByIdsAsync(
        IEnumerable<Guid> imageIds
    )
    {
        throw new NotImplementedException();
    }
}
