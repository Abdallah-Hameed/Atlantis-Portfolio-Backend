using Atlantis_Portfolio_Backend_BLL;
using Atlantis_Portfolio_Backend.DTO;
using Atlantis_Portfolio_Backend_DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlantis_Portfolio_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SocialLinkController : ControllerBase
{
    private readonly clsSocialLinkService _socialLinkService;

    public SocialLinkController(clsSocialLinkService socialLinkService)
    {
        _socialLinkService = socialLinkService;
    }

    [AllowAnonymous]
    [HttpGet("Get/All")]
    [ProducesResponseType(typeof(List<SocialLink>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSocialLinks()
    {
        var socialLinks = await _socialLinkService.GetAllSocialLinksAsync();

        return Ok(socialLinks);
    }

    [Authorize]
    [HttpGet("Get/{socialLinkId:int}")]
    [ProducesResponseType(typeof(SocialLink), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSocialLinkById(int socialLinkId)
    {
        if (socialLinkId <= 0)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid social link ID", detail: "Social link ID must be greater than zero.");

        var socialLink = await _socialLinkService.GetSocialLinkByIdAsync(socialLinkId);

        if (socialLink == null)
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Social link not found", detail: $"Social link with ID {socialLinkId} was not found.");

        return Ok(socialLink);
    }

    [Authorize]
    [HttpPost("Add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddSocialLink([FromBody] clsAddSocialLinkDto dto)
    {
        var result = await _socialLinkService.AddSocialLinkAsync(dto.Platform, dto.Url, dto.DisplayOrder);

        return result switch
        {
            enOperationResult.Success => Ok("Social link added successfully."),
            enOperationResult.InvalidData => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid social link data", detail: "The provided social link data is invalid."),
            enOperationResult.AlreadyExist => Problem(statusCode: StatusCodes.Status409Conflict, title: "Social link already exists", detail: "A social link with the same URL already exists."),
            enOperationResult.Conflict => Problem(statusCode: StatusCodes.Status409Conflict, title: "Social link conflict", detail: "The requested display order is already in use, or a database constraint prevented the operation."),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", detail: "Failed to add social link.")
        };
    }

    [Authorize]
    [HttpPut("Update/{socialLinkId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateSocialLink(int socialLinkId, [FromBody] clsUpdateSocialLinkDto dto)
    {
        var result = await _socialLinkService.UpdateSocialLinkAsync(socialLinkId, dto.Platform, dto.Url, dto.DisplayOrder);

        return result switch
        {
            enOperationResult.Success => Ok("Social link updated successfully."),
            enOperationResult.InvalidData => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid social link data", detail: "The social link ID must be positive and the supplied data must be valid."),
            enOperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, title: "Social link not found", detail: $"Social link with ID {socialLinkId} was not found."),
            enOperationResult.AlreadyExist => Problem(statusCode: StatusCodes.Status409Conflict, title: "Social link already exists", detail: "Another social link with the same URL already exists."),
            enOperationResult.Conflict => Problem(statusCode: StatusCodes.Status409Conflict, title: "Social link conflict", detail: "The requested display order is already in use, or a database constraint prevented the operation."),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", detail: "Failed to update social link.")
        };
    }

    [Authorize]
    [HttpDelete("Delete/{socialLinkId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteSocialLink(int socialLinkId)
    {
        var result = await _socialLinkService.DeleteSocialLinkAsync(socialLinkId);

        return result switch
        {
            enOperationResult.Success => Ok("Social link deleted successfully."),
            enOperationResult.InvalidData => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid social link ID", detail: "Social link ID must be greater than zero."),
            enOperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, title: "Social link not found", detail: $"Social link with ID {socialLinkId} was not found."),
            enOperationResult.Conflict => Problem(statusCode: StatusCodes.Status409Conflict, title: "Social link deletion conflict", detail: "The social link could not be deleted because a database constraint prevented the operation."),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", detail: "Failed to delete social link.")
        };
    }
}