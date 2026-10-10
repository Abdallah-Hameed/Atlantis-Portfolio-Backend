using Atlantis_Portfolio_Backend_BLL;
using Atlantis_Portfolio_Backend.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlantis_Portfolio_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EducationController : ControllerBase
{
    private readonly clsEducationService _educationService;

    public EducationController(clsEducationService educationService)
    {
        _educationService = educationService;
    }

    [AllowAnonymous]
    [HttpGet("Get/All")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllEducations()
    {
        var educations = await _educationService.GetAllEducationsAsync();

        return Ok(educations);
    }

    [Authorize]
    [HttpGet("Get/{educationId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEducationById(int educationId)
    {
        var education = await _educationService.GetEducationByIdAsync(educationId);

        if (education == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Education not found",
                detail: $"Education with ID {educationId} not found.");
        }

        return Ok(education);
    }

    [Authorize]
    [HttpPost("Add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddEducation([FromBody] clsAddEducationDto dto)
    {
        var result = await _educationService.AddEducationAsync(
            dto.Institution,
            dto.Degree,
            dto.FieldOfStudy,
            dto.Description,
            dto.StartDate,
            dto.EndDate,
            dto.IsCurrent);

        return result switch
        {
            enOperationResult.Success =>
                Ok("Education added successfully."),

            enOperationResult.InvalidData =>
                Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid education data",
                    detail: "The provided education data is invalid."),

            enOperationResult.Conflict =>
                Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Education conflict",
                    detail: "A conflict occurred while saving the education. Please try again."),

            _ =>
                Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal server error",
                    detail: "Failed to add education.")
        };
    }

    [Authorize]
    [HttpPut("Update/{educationId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateEducation(
        int educationId,
        [FromBody] clsUpdateEducationDto dto)
    {
        var result = await _educationService.UpdateEducationAsync(
            educationId,
            dto.Institution,
            dto.Degree,
            dto.FieldOfStudy,
            dto.Description,
            dto.StartDate,
            dto.EndDate,
            dto.IsCurrent);

        return result switch
        {
            enOperationResult.Success =>
                Ok("Education updated successfully."),

            enOperationResult.InvalidData =>
                Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid education data",
                    detail: "The provided education data is invalid."),

            enOperationResult.NotFound =>
                Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Education not found",
                    detail: $"Education with ID {educationId} not found."),

            _ =>
                Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal server error",
                    detail: "Failed to update education.")
        };
    }

    [Authorize]
    [HttpDelete("Delete/{educationId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteEducation(int educationId)
    {
        var result = await _educationService.DeleteEducationAsync(educationId);

        return result switch
        {
            enOperationResult.Success =>
                Ok("Education deleted successfully."),

            enOperationResult.InvalidData =>
                Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid education ID",
                    detail: "Education ID must be greater than zero."),

            enOperationResult.NotFound =>
                Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Education not found",
                    detail: $"Education with ID {educationId} not found."),

            enOperationResult.Conflict =>
                Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Education deletion conflict",
                    detail: "The education could not be deleted because of a conflict with related records."),

            _ =>
                Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal server error",
                    detail: "Failed to delete education.")
        };
    }
}