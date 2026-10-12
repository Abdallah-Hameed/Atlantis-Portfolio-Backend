using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Atlantis_Portfolio_Backend_BLL;

namespace Atlantis_Portfolio_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfolioProfileController : ControllerBase
{
    private readonly clsPortfolioProfileService _profileService;

    public PortfolioProfileController(clsPortfolioProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("Get/Profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await _profileService.GetProfileAsync();

        if (profile == null)
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Portfolio profile not found", detail: "No portfolio profile was found.");

        var responseDto = new clsPortfolioProfileDto
        {
            PortfolioProfileId = profile.PortfolioProfileId,

            FullName = profile.FullName,

            ProfessionalTitle = profile.ProfessionalTitle,

            Bio = profile.Bio,

            ProfileImageUrl = profile.ProfileImageUrl,

            Email = profile.Email,

            Location = profile.Location
        };

        return Ok(responseDto);
    }

    [HttpPost("Add")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddProfile([FromBody] clsAddPortfolioProfileDto dto)
    {
        var (result, profileId) = await _profileService.AddProfileAsync(dto.FullName, dto.ProfessionalTitle, dto.Bio, dto.ProfileImageUrl, dto.Email, dto.Location);

        return result switch
        {
            enOperationResult.Success =>
                StatusCode(StatusCodes.Status201Created, new { ProfileId = profileId }),

            enOperationResult.InvalidData =>
                Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid profile data",
                    detail: "The provided profile data is invalid."),

            enOperationResult.Conflict =>
                Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Portfolio profile already exists",
                    detail: "A portfolio profile already exists. Only one profile is allowed."),

            _ =>
                Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal server error",
                    detail: "Failed to create profile.")
        };
    }

    [HttpPut("Update/{profileId:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProfile(int profileId, [FromBody] clsUpdatePortfolioProfileDto dto)
    {
        var result = await _profileService.UpdateProfileAsync(profileId, dto.FullName, dto.ProfessionalTitle, dto.Bio, dto.ProfileImageUrl, dto.Email, dto.Location);

        return result switch
        {
            enOperationResult.Success => Ok("Portfolio profile updated successfully."),

            enOperationResult.InvalidData => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid profile data", detail: "The provided profile data is invalid."),

            enOperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, title: "Portfolio profile not found", detail: $"Portfolio profile with ID {profileId} not found."),

            _ => Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", detail: "Failed to update profile.")
        };
    }
}