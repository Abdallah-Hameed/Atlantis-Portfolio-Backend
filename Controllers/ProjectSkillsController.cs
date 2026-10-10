using Atlantis_Portfolio_Backend_BLL;
using Atlantis_Portfolio_Backend.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlantis_Portfolio_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectSkillController : ControllerBase
{
    private readonly clsProjectSkillService _projectSkillService;

    public ProjectSkillController(clsProjectSkillService projectSkillService)
    {
        _projectSkillService = projectSkillService;
    }

    [Authorize]
    [HttpGet("Get/All")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProjectSkills()
    {
        var projectSkills = await _projectSkillService.GetAllProjectSkillsAsync();

        var result = projectSkills.Select(ps => new clsProjectSkillDto
        {
            ProjectSkillId = ps.ProjectSkillId,
            ProjectId = ps.ProjectId,
            SkillId = ps.SkillId,
            ProjectName = ps.Project.Title,
            SkillName = ps.Skill.SkillName
        }).ToList();

        return Ok(result);
    }

    [Authorize]
    [HttpGet("Get/{projectId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectSkills(int projectId)
    {
        var projectSkills = await _projectSkillService.GetProjectSkillsByIDAsync(projectId);

        var result = projectSkills.Select(ps => new clsProjectSkillDto
        {
            ProjectSkillId = ps.ProjectSkillId,
            ProjectId = ps.ProjectId,
            SkillId = ps.SkillId,
            ProjectName = ps.Project.Title,
            SkillName = ps.Skill.SkillName
        }).ToList();

        return Ok(result);
    }

    [Authorize]
    [HttpPost("Add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddProjectSkill([FromBody] clsAddProjectSkillDto dto)
    {
        var result = await _projectSkillService.AddProjectSkillAsync(dto.ProjectId, dto.SkillId);

        return result switch
        {
            enOperationResult.Success =>
                Ok("Project skill added successfully."),

            enOperationResult.InvalidData =>
                Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid project skill data", detail: "The provided project skill data is invalid."),

            enOperationResult.NotFound =>
                Problem(statusCode: StatusCodes.Status404NotFound, title: "Project skill reference not found", detail: "The referenced project or skill was not found."),

            enOperationResult.AlreadyExist =>
                Problem(statusCode: StatusCodes.Status409Conflict, title: "Project skill already exists", detail: "The project skill association already exists."),

            _ =>
                Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", detail: "Failed to add project skill.")
        };
    }

    [Authorize]
    [HttpDelete("Delete/{projectSkillId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProjectSkill(int projectSkillId)
    {
        var result = await _projectSkillService.DeleteProjectSkillAsync(projectSkillId);

        return result switch
        {
            enOperationResult.Success =>
                Ok("Project skill deleted successfully."),

            enOperationResult.InvalidData =>
                Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid project skill ID", detail: "Project skill ID must be greater than zero."),

            enOperationResult.NotFound =>
                Problem(statusCode: StatusCodes.Status404NotFound, title: "Project skill not found", detail: $"Project skill with ID {projectSkillId} not found."),

            _ =>
                Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", detail: "Failed to delete project skill.")
        };
    }
}