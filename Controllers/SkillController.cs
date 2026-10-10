using Atlantis_Portfolio_Backend_BLL;
using Atlantis_Portfolio_Backend.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlantis_Portfolio_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillController : ControllerBase
{
    private readonly clsSkillService _skillService;

    public SkillController(clsSkillService skillService)
    {
        _skillService = skillService;
    }

    [Authorize]
    [HttpGet("Get/All")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSkills()
    {
        var skills = await _skillService.GetAllSkillsAsync();
        return Ok(skills);
    }

    [Authorize]
    [HttpGet("Get/{skillId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSkillById(int skillId)
    {
        var skill = await _skillService.GetSkillByIdAsync(skillId);

        if (skill == null)
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Skill not found", detail: $"Skill with ID {skillId} not found.");

        return Ok(skill);
    }

    [Authorize]
    [HttpPost("Add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddSkill([FromBody] clsAddSkillDto dto)
    {
        var result = await _skillService.AddSkillAsync(dto.SkillName, dto.SkillCategory, dto.SkillLevel);

        return result switch
        {
            enOperationResult.Success => Ok("Skill added successfully."),

            enOperationResult.InvalidData => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid skill data", detail: "The provided skill data is invalid."),

            enOperationResult.InvalidSkillLevel => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid skill level", detail: "Skill level must be between 1 and 5."),

            enOperationResult.AlreadyExist => Problem(statusCode: StatusCodes.Status409Conflict, title: "Skill already exists", detail: "A skill with the same unique values already exists."),

            _ => Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", detail: "Failed to add skill.")
        };
    }

    [Authorize]
    [HttpPut("Update/{skillId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateSkill(int skillId, [FromBody] clsUpdateSkillDto dto)
    {
        var result = await _skillService.UpdateSkillAsync(skillId, dto.SkillName, dto.SkillCategory, dto.SkillLevel);

        return result switch
        {
            enOperationResult.Success => Ok("Skill updated successfully."),

            enOperationResult.InvalidData => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid skill data", detail: "The provided skill data is invalid."),

            enOperationResult.InvalidSkillLevel => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid skill level", detail: "Skill level must be between 1 and 5."),

            enOperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, title: "Skill not found", detail: $"Skill with ID {skillId} not found."),

            enOperationResult.AlreadyExist => Problem(statusCode: StatusCodes.Status409Conflict, title: "Skill already exists", detail: "A skill with the same unique values already exists."),

            _ => Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", detail: "Failed to update skill.")
        };
    }

    [Authorize]
    [HttpDelete("Delete/{skillId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteSkill(int skillId)
    {
        var result = await _skillService.DeleteSkillAsync(skillId);

        return result switch
        {
            enOperationResult.Success => Ok("Skill deleted successfully."),

            enOperationResult.InvalidData => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid skill ID", detail: "Skill ID must be greater than zero."),

            enOperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, title: "Skill not found", detail: $"Skill with ID {skillId} not found."),

            enOperationResult.Conflict => Problem(statusCode: StatusCodes.Status409Conflict, title: "Skill deletion conflict", detail: "The skill could not be deleted because of a conflict with related records."),

            _ => Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", detail: "Failed to delete skill.")
        };
    }
}