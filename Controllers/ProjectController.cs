using Atlantis_Portfolio_Backend.DTO;
using Atlantis_Portfolio_Backend.DTOs.Project;
using Atlantis_Portfolio_Backend_BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlantis_Portfolio_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly clsProjectService _projectService;

    public ProjectController(clsProjectService projectService)
    {
        _projectService = projectService;
    }

    [AllowAnonymous]
    [HttpGet("Get/All")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProjects()
    {
        var projects = await _projectService.GetAllProjectsAsync();

        return Ok(projects);
    }

    [Authorize]
    [HttpGet("Get/{projectId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectById(int projectId)
    {
        var project = await _projectService.GetProjectByIdAsync(projectId);

        if (project == null)
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Project not found", detail: $"Project with ID {projectId} not found.");

        return Ok(project);
    }

    [Authorize]
    [HttpPost("Add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddProject([FromBody] clsAddProjectDto dto)
    {
        var result = await _projectService.AddProjectAsync(dto.Title, dto.Description, dto.ImageUrl, dto.GitHubUrl, dto.LiveUrl);

        return result switch
        {
            enOperationResult.Success => Ok("Project added successfully."),
            enOperationResult.InvalidData => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid project data", detail: "The provided project data is invalid."),
            enOperationResult.Conflict => Problem(statusCode: StatusCodes.Status409Conflict, title: "Project conflict", detail: "A conflict occurred while saving the project. Please try again."),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", detail: "Failed to add project.")
        };
    }

    [Authorize]
    [HttpPut("Update/{projectId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProject(int projectId, [FromBody] clsUpdateProjectDto dto)
    {
        var result = await _projectService.UpdateProjectAsync(projectId, dto.Title, dto.Description, dto.ImageUrl, dto.GitHubUrl, dto.LiveUrl);

        return result switch
        {
            enOperationResult.Success => Ok("Project updated successfully."),
            enOperationResult.InvalidData => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid project data", detail: "The provided project data is invalid."),
            enOperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, title: "Project not found", detail: $"Project with ID {projectId} not found."),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", detail: "Failed to update project.")
        };
    }

    [Authorize]
    [HttpDelete("Delete/{projectId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProject(int projectId)
    {
        var result = await _projectService.DeleteProjectAsync(projectId);

        return result switch
        {
            enOperationResult.Success => Ok("Project deleted successfully."),
            enOperationResult.InvalidData => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid project ID", detail: "Project ID must be greater than zero."),
            enOperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, title: "Project not found", detail: $"Project with ID {projectId} not found."),
            enOperationResult.Conflict => Problem(statusCode: StatusCodes.Status409Conflict, title: "Project deletion conflict", detail: "The project could not be deleted because of a conflict with related records."),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", detail: "Failed to delete project.")
        };
    }

    [Authorize]
    [HttpPost("Reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Reorder([FromBody] List<clsReorderProjectItemDto> items)
    {
        if (items == null || items.Count == 0)
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid data",
                detail: "The reorder list is empty.");

        var payload = items
            .Select(x => (x.ProjectId, x.DisplayOrder))
            .ToList();

        var result = await _projectService.ReorderProjectsAsync(payload);

        return result switch
        {
            enOperationResult.Success =>
                Ok("Projects reordered successfully."),

            enOperationResult.InvalidData =>
                Problem(statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid reorder data",
                    detail: "One or more items have invalid values."),

            enOperationResult.NotFound =>
                Problem(statusCode: StatusCodes.Status404NotFound,
                    title: "Project not found",
                    detail: "One or more project IDs do not exist."),

            enOperationResult.AlreadyExist =>
                Problem(statusCode: StatusCodes.Status409Conflict,
                    title: "Order conflict",
                    detail: "The new order conflicts with an existing constraint."),

            _ =>
                Problem(statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal server error",
                    detail: "Failed to reorder projects.")
        };
    }
}