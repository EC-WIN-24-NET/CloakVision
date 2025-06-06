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
[Route("api/[controller]")]
public class ImageController(IImageService imageService, IWebHostEnvironment webHostEnvironment)
    : ControllerBase
{
    /// <summary>
    /// Get Image by Guid
    /// /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    [HttpGet("{guid:Guid}", Name = "GetEventByGuid")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetEventByGuid([FromRoute] Guid guid)
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
    /// Get All Event
    /// /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("GetAllEvents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetAllEventsAsync()
    {
        try
        {
            // Get the Events from the database
            var eventsAsync = await imageService.GetAllImagesAsync();
            // Return the events
            return eventsAsync.Value != null && eventsAsync.Value.Any()
                ? ApiResponseHelper.Success(eventsAsync)
                : ApiResponseHelper.NotFound("No projects found");
        }
        catch (Exception ex)
        {
            // Return a problem response, in development mode, it will include the stack trace
            return ApiResponseHelper.Problem(ex, webHostEnvironment.IsDevelopment());
        }
    }
}
