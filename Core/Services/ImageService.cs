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
    /// Retrieves all events from the repository and converts them to display DTOs.
    /// Because we are filtering out null events, we can use the `e => e != null` predicate.
    /// and the rest of error handling is done in the repository.
    /// </summary>
    /// <returns></returns>
    public async Task<RepositoryResult<IEnumerable<ImageDisplay>>> GetAllImagesAsync()
    {
        try
        {
            // Get all events from the repository
            var getEvents = await imageRepository.GetAllAsync(e => e != null);
            // Check if the repository operation itself failed
            var eventDisplayDTos = getEvents
                // Filter out any null events, if necessary
                .Where(e => e != null)
                .Select(e => imageDtoFactory.ToDisplay(e!))
                .ToList();
            // Return the result using the result factory
            return resultFactory.OperationSuccess<IEnumerable<ImageDisplay>>(eventDisplayDTos, 200);
        }
        // Case 1: Repository operation itself failed (e.g., DB connection issue, invalid query).
        catch (Exception ex)
        {
            // Log the exception ex (recommended)
            return resultFactory.OperationFailed<IEnumerable<ImageDisplay>>(
                new Error(
                    "ImageService.GetAll.Exception",
                    $"An unexpected error occurred while retrieving all events: {ex.Message}"
                ),
                500 // Internal Server Error
            );
        }
    }

    public async Task<RepositoryResult<ImageDisplay>> GetImageByGuid(Guid id)
    {
        try
        {
            // Get the event from the repository
            var eventResult = await imageRepository.GetAsync(e => e != null && e.Id == id, false);

            // Case 1: Repository operation itself failed (e.g., DB connection issue, invalid query).
            // eventResult.Error will be a specific error type, not Error.NonError.
            if (eventResult.Error != Error.NonError)
                return resultFactory.OperationFailed<ImageDisplay>(
                    eventResult.Error,
                    eventResult.StatusCode
                );

            // Case 2: Entity was found.
            // BaseRepository.GetAsync returns Value != null and StatusCode = 200.
            // This means the event was successfully retrieved.
            if (eventResult.Value != null)
            {
                var displayEventDto = imageDtoFactory.ToDisplay(eventResult.Value);
                return resultFactory.OperationSuccess(displayEventDto, eventResult.StatusCode);
            }

            // Case 3: Entity was not found.
            if (eventResult.StatusCode == 404)
                return resultFactory.OperationFailed<ImageDisplay>(
                    Error.NotFound("Event is not found"),
                    404
                );

            // Case 4: Unexpected state after retrieving event details.
            return resultFactory.OperationFailed<ImageDisplay>(
                new Error(
                    "ImageService.UnexpectedState",
                    "An unexpected state was encountered after retrieving event details."
                ),
                500 // Or eventResult.StatusCode if it's a known non-error, non-404 code
            );
        }
        catch (Exception)
        {
            // Log the exception ex (recommended)
            return resultFactory.OperationFailed<ImageDisplay>(
                new Error(
                    "Event.RetrievalError",
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
