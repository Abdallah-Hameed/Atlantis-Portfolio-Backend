using Atlantis_Portfolio_Backend_BLL;
using Atlantis_Portfolio_Backend.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlantis_Portfolio_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExperienceController : ControllerBase
{
    private readonly clsExperienceService _experienceService;

    public ExperienceController(clsExperienceService experienceService)
    {
        _experienceService = experienceService;
    }

    [AllowAnonymous]
    [HttpGet("Get/All")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllExperiences()
    {
        var experiences = await _experienceService.GetAllExperiencesAsync();

        return Ok(experiences);
    }

    [Authorize]
    [HttpGet("Get/{experienceId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExperienceById(int experienceId)
    {
        var experience = await _experienceService.GetExperienceByIdAsync(experienceId);

        if (experience == null)
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Experience not found",
                detail: $"Experience with ID {experienceId} not found.");

        return Ok(experience);
    }

    [Authorize]
    [HttpPost("Add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddExperience([FromBody] clsAddExperienceDto dto)
    {
        var result = await _experienceService.AddExperienceAsync(
            dto.Company,
            dto.Position,
            dto.Description,
            dto.StartDate,
            dto.EndDate,
            dto.IsCurrent);

        return result switch
        {
            enOperationResult.Success =>
                Ok("Experience added successfully."),

            enOperationResult.InvalidData =>
                Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid experience data",
                    detail: "The provided experience data is invalid."),

            enOperationResult.Conflict =>
                Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Experience conflict",
                    detail: "A conflict occurred while saving the experience. Please try again."),

            _ =>
                Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal server error",
                    detail: "Failed to add experience.")
        };
    }

    [Authorize]
    [HttpPut("Update/{experienceId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateExperience(
        int experienceId,
        [FromBody] clsUpdateExperienceDto dto)
    {
        var result = await _experienceService.UpdateExperienceAsync(
            experienceId,
            dto.Company,
            dto.Position,
            dto.Description,
            dto.StartDate,
            dto.EndDate,
            dto.IsCurrent);

        return result switch
        {
            enOperationResult.Success =>
                Ok("Experience updated successfully."),

            enOperationResult.InvalidData =>
                Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid experience data",
                    detail: "The provided experience data is invalid."),

            enOperationResult.NotFound =>
                Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Experience not found",
                    detail: $"Experience with ID {experienceId} not found."),

            _ =>
                Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal server error",
                    detail: "Failed to update experience.")
        };
    }

    [Authorize]
    [HttpDelete("Delete/{experienceId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteExperience(int experienceId)
    {
        var result = await _experienceService.DeleteExperienceAsync(experienceId);

        return result switch
        {
            enOperationResult.Success =>
                Ok("Experience deleted successfully."),

            enOperationResult.InvalidData =>
                Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid experience ID",
                    detail: "Experience ID must be greater than zero."),

            enOperationResult.NotFound =>
                Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Experience not found",
                    detail: $"Experience with ID {experienceId} not found."),

            enOperationResult.Conflict =>
                Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Experience deletion conflict",
                    detail: "The experience could not be deleted because of a conflict with related records."),

            _ =>
                Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal server error",
                    detail: "Failed to delete experience.")
        };
    }
}