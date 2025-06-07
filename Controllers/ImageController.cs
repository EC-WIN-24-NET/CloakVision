using CloakVision.Helpers;
using CloakVision.Interface;
using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CloakVision.Controllers;

/// <summary>
/// ImageController
/// </summary>
[ApiController]
[Route("/[controller]")]
public class ImageController(IImageService imageService, IWebHostEnvironment webHostEnvironment)
    : ControllerBase
{
    /// <summary>
    /// Get Image by Guid
    /// /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    [HttpGet("{guid:Guid}", Name = "GetImageByGuid")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetImageByGuid([FromRoute] Guid guid)
    {
        try
        {
            // Validate the Guid
            if (guid == Guid.Empty)
                return ApiResponseHelper.BadRequest("Invalid Guid provided.");

            // Get the status from the database
            var status = await imageService.GetImageByGuid(guid);
            // Return the status
            return ApiResponseHelper.Success(status);
        }
        catch (Exception ex)
        {
            // Return a problem response, in development mode, it will include the stack trace
            return ApiResponseHelper.Problem(ex, webHostEnvironment.IsDevelopment());
        }
    }

    /// <summary>
    /// Get All Image
    /// /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("GetAllImages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetAllImagesAsync()
    {
        try
        {
            // Get the Images from the database
            var ImagesAsync = await imageService.GetAllImagesAsync();
            // Return the Images
            return ImagesAsync.Value != null && ImagesAsync.Value.Any()
                ? ApiResponseHelper.Success(ImagesAsync)
                : ApiResponseHelper.NotFound("No projects found");
        }
        catch (Exception ex)
        {
            // Return a problem response, in development mode, it will include the stack trace
            return ApiResponseHelper.Problem(ex, webHostEnvironment.IsDevelopment());
        }
    }
}
