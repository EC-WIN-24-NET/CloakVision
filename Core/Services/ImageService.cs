using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Data;
using Core.Interfaces.Factories;
using Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Services;

public class ImageService(
    IImageRepository imageRepository,
    IRepositoryResultFactory resultFactory,
    IImageDtoFactory imageDtoFactory,
    IConfiguration configuration,
    BlobServiceClient blobServiceClient,
    ILogger<ImageService> logger
) : IImageService
{
    private readonly string? _blobContainerName = configuration["AzureBlobStorage:ContainerName"];

    /// <summary>
    /// Retrieves all images from the repository and converts them to display DTOs.
    /// /// Because we are filtering out null images, we can use the `e => e != null` predicate.
    /// and the rest of error handling is done in the repository.
    /// /// </summary>
    /// <returns></returns>
    public async Task<RepositoryResult<IEnumerable<ImageDisplay>>> GetAllImagesAsync()
    {
        try
        {
            // Log the retrieval attempt
            logger.LogInformation("Retrieving all images.");
            // Get all images from the repository
            var imageEntities = await imageRepository.GetAllAsync(e => e != null);

            // Initialize a list to hold the ImageDisplay DTOs
            var displayDtoImage = new List<ImageDisplay>();

            // Loop through each image entity and convert it to ImageDisplay DTO
            foreach (var imageEntity in imageEntities.Where(e => e != null))
            {
                // Convert the image entity to ImageDisplay DTO
                var imageDisplayDto = imageDtoFactory.ToDisplay(imageEntity!);
                // Extract the final image path from the DTO
                var finalImagePath = imageDisplayDto.Path;

                // Check if the finalImagePath is not null or empty and does not start with "http"
                if (
                    !string.IsNullOrWhiteSpace(finalImagePath)
                    && !finalImagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                )
                {
                    // finalImagePath is a relative path, we need to generate a SAS URL.
                    if (string.IsNullOrWhiteSpace(_blobContainerName))
                    {
                        logger.LogError(
                            "Blob container name is not configured. Cannot generate SAS URL for image ID: {Id} with relative path: {Path}",
                            imageEntity!.Id,
                            finalImagePath
                        );
                        // We are skipping this image since the container name is not configured.
                        continue;
                    }

                    // The Stored path is relative, and container name is configured. Attempt SAS URL generation.
                    try
                    {
                        // Assuming finalPath from DB is the relative name
                        var relativeBlobName = finalImagePath;
                        // Generate the SAS URL for the blob
                        finalImagePath = await GenerateUserDelegationSasUrl(
                            _blobContainerName,
                            relativeBlobName
                        );
                    }
                    // Catch any exceptions that occur during SAS URL generation
                    catch (Exception ex)
                    {
                        logger.LogError(
                            ex,
                            "Error generating SAS URL for image ID: {ImageId}. Relative Path: {Path}",
                            imageEntity!.Id,
                            imageDisplayDto.Path
                        );
                        // We are skipping this image since the container name is not configured.
                        continue;
                    }
                }

                // Create new ImageDisplay (new list item) with the potentially updated path
                displayDtoImage.Add(
                    new ImageDisplay
                    {
                        Id = imageDisplayDto.Id,
                        Name = imageDisplayDto.Name,
                        Path = finalImagePath, // This will be SAS URL or original path
                        Description = imageDisplayDto.Description,
                        CreatedAt = imageDisplayDto.CreatedAt,
                        AltText = imageDisplayDto.AltText,
                    }
                );
            }

            logger.LogInformation(
                "Successfully fetched {ImageCount} images.",
                displayDtoImage.Count
            );
            return resultFactory.OperationSuccess<IEnumerable<ImageDisplay>>(displayDtoImage, 200);
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

    /// <summary>
    /// Retrieves an image by its unique identifier (GUID) from the repository.
    /// Handles various scenarios such as image not found, repository errors, and SAS URL generation for relative paths.
    /// This code was refactored by Google Gemini AI Pro to improve clarity and adding support for SAS URL generation.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the image to retrieve.</param>
    /// <returns>
    /// A <see cref="RepositoryResult{ImageDisplay}"/> containing the image details if found, or an error result if not.
    /// Possible status codes:
    /// - 200: Successfully retrieved the image.
    /// - 404: Image not found.
    /// - 500: Internal server error.
    /// </returns>
    public async Task<RepositoryResult<ImageDisplay>> GetImageByGuid(Guid id)
    {
        try
        {
            // Log the retrieval attempt
            logger.LogInformation("Retrieving image with ID: {ImageId}", id);

            // Get the images from the repository
            var imageResult = await imageRepository.GetAsync(e => e != null && e.Id == id, false);

            // Case 1: Repository operation itself failed (e.g., DB connection issue, invalid query).
            // imageResult.Error will be a specific error type, not Error.NonError.
            if (imageResult.Error != Error.NonError)
            {
                // Log the error with the specific error code
                logger.LogWarning(
                    "Error fetching image with ID: {Id} from repository. Error: {Error}",
                    id,
                    imageResult.Error.ErrorCode
                );
                // Return the error result using the result factory
                return resultFactory.OperationFailed<ImageDisplay>(
                    imageResult.Error,
                    imageResult.StatusCode
                );
            }

            // Case 2: Entity was found or not found.
            // BaseRepository.GetAsync returns Value != null and StatusCode = 200.
            // This means the image was successfully retrieved.
            if (imageResult.Value == null || imageResult.StatusCode == 404)
            {
                logger.LogWarning("Image with ID: {Id} not found.", id);
                return resultFactory.OperationFailed<ImageDisplay>(
                    Error.NotFound($"Image with ID {id} not found."),
                    404
                );
            }

            // Case 3: Successfully retrieved image details.
            var imageDisplayDto = imageDtoFactory.ToDisplay(imageResult.Value);
            var finalImagePath = imageDisplayDto.Path;

            // If the image path is not a valid URL, we need to generate a SAS URL.
            // Check if the finalImagePath is not null or empty and does not start with "http"
            if (
                // finalImagePath is not null, not empty, and does not start with "http"
                !string.IsNullOrWhiteSpace(finalImagePath)
                && !finalImagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            )
            {
                // finalImagePath is a relative path, we need to generate a SAS URL.
                if (string.IsNullOrWhiteSpace(_blobContainerName))
                {
                    logger.LogError(
                        "Blob container name is not configured. Cannot generate SAS URL for image ID: {Id} with relative path: {Path}",
                        id,
                        finalImagePath
                    );
                    return resultFactory.OperationFailed<ImageDisplay>(
                        new Error(
                            "ImageService.ConfigurationError",
                            "Blob container name is not configured, cannot form absolute URL for image."
                        ),
                        500
                    );
                }

                // The Stored path is relative, and container name is configured. Attempt SAS URL generation.
                try
                {
                    // Assuming finalPath from DB is the relative name
                    var relativeBlobName = finalImagePath;
                    finalImagePath = await GenerateUserDelegationSasUrl(
                        _blobContainerName,
                        relativeBlobName
                    );
                }
                catch (Exception ex)
                {
                    // The 403 error you saw would be caught here if permissions are still an issue
                    logger.LogError(
                        ex,
                        "Error generating SAS URL for image ID: {Id}. Relative Path: {Path}",
                        id,
                        finalImagePath
                    );
                    return resultFactory.OperationFailed<ImageDisplay>(
                        new Error(
                            "image.SasUrlGenerationError",
                            $"An error occurred while generating the SAS URL for the image: {ex.Message}" // Include ex.Message
                        ),
                        500
                    );
                }
            }

            // Create a new ImageDisplay instance with the potentially updated path
            var imageDisplayWithSas = new ImageDisplay
            {
                Id = imageDisplayDto.Id,
                Name = imageDisplayDto.Name,
                Path = finalImagePath,
                Description = imageDisplayDto.Description,
                CreatedAt = imageDisplayDto.CreatedAt,
                AltText = imageDisplayDto.AltText,
            };
            logger.LogInformation("Successfully fetched image with ID: {Id}", id);
            return resultFactory.OperationSuccess(imageDisplayWithSas, 200);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in GetImageByGuid for ID: {Id}", id);
            return resultFactory.OperationFailed<ImageDisplay>(
                new Error(
                    "ImageService.GetById.Exception",
                    $"An unexpected error occurred: {ex.Message}"
                ),
                500
            );
        }
    }

    public Task<RepositoryResult<IEnumerable<ImageDisplay>>> GetImagesByIdsAsync(
        IEnumerable<Guid> imageIds
    )
    {
        // Implement the similar logic as GetAllImagesAsync, but filter by imageIds
        // You might want to fetch images from the repository in a batch if possible.
        logger.LogWarning("GetImagesByIdsAsync is not fully implemented.");
        throw new NotImplementedException(
            "GetImagesByIdsAsync needs to be implemented with SAS URL generation."
        );
    }

    public Task<RepositoryResult<ImageDisplay>> CreateImageAsync(ImageCreate imageCreateDto)
    {
        // TODO: Implement the logic to create an image.
        throw new NotImplementedException();
    }

    /// <summary>
    ///  THis code was partially generated by Google Gemini AI Pro.
    ///  Generates a User Delegation SAS URL for a blob in Azure Blob Storage.
    ///  This method uses the BlobServiceClient to create a SAS URL that allows read access to the specified blob.
    ///  The SAS URL is valid for a specified number of minutes and enforces HTTPS.
    ///  This entire code block was Generated by Google Gemini AI Pro
    /// </summary>
    /// <param name="containerName"></param>
    /// <param name="blobName"></param>
    /// <param name="minutesToExpire"></param>
    /// <returns></returns>
    private async Task<string> GenerateUserDelegationSasUrl(
        string containerName,
        string blobName,
        int minutesToExpire = 60
    )
    {
        // Validate parameters
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        // Check if the container exists
        if (!await blobClient.ExistsAsync())
        {
            logger.LogWarning(
                "Blob {BlobName} in container {ContainerName} does not exist. Cannot generate SAS URL.",
                blobName,
                containerName
            );
            // Return the original blobName or an empty string, or throw, depending on desired behavior.
            // Returning the original path might be confusing if the client expects a full URL.
            // For now, let's return the original path as a fallback, but this should be reviewed.
            return blobName;
        }

        // Generate a User Delegation Key for the Blob Service Client
        // Start time 5 mins in past to allow for clock skew
        UserDelegationKey userDelegationKey = await blobServiceClient.GetUserDelegationKeyAsync(
            DateTimeOffset.UtcNow.AddMinutes(-5),
            DateTimeOffset.UtcNow.AddMinutes(minutesToExpire + 5)
        ); // Key validity slightly longer than SAS

        // Create a BlobSasBuilder to generate the SAS URL
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobName,
            Resource = "b", // "b" for blob
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // Allow for clock skew
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(minutesToExpire),
            Protocol = SasProtocol.Https, // Enforce HTTPS
        };

        // Set the permissions for the SAS URL
        sasBuilder.SetPermissions(BlobSasPermissions.Read); // Read-only access

        // Build the SAS URL using the BlobUriBuilder
        var blobUriBuilder = new BlobUriBuilder(blobClient.Uri)
        {
            Sas = sasBuilder.ToSasQueryParameters(userDelegationKey, blobServiceClient.AccountName),
        };

        // Log the generated SAS URL for debugging purposes
        logger.LogInformation(
            "Generated SAS URL for blob: {BlobName} in container {ContainerName}",
            blobName,
            containerName
        );
        return blobUriBuilder.ToUri().ToString();
    }
}
