using Atlantis_Portfolio_Backend_BLL;
using Atlantis_Portfolio_Backend_BLL.Services;
using Atlantis_Portfolio_Backend_DAL;
using Atlantis_Portfolio_Backend_DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlantis_Portfolio_Backend_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ActionController : ControllerBase
{
    private readonly clsActionService _actionService;

    public ActionController(clsActionService actionService)
    {
        _actionService = actionService;
    }

    [Authorize]
    [HttpGet("Get/All")]
    [ProducesResponseType(typeof(List<Atlantis_Portfolio_Backend_DAL.Entities.Action>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var actions = await _actionService.GetAllAsync();

            return Ok(actions);
        }
        catch
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error",
                detail: "An error occurred while retrieving Actions.");
        }
    }
    [Authorize]
    [HttpGet("Get/{actionId:int}")]
    [ProducesResponseType(typeof(Atlantis_Portfolio_Backend_DAL.Entities.Action), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int actionId)
    {
        try
        {
            var action = await _actionService.GetByIdAsync(actionId);

            if (action == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    detail: "Action was not found.");
            }

            return Ok(action);
        }
        catch
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error",
                detail: "An error occurred while retrieving the Action.");
        }
    }
    [Authorize]
    [HttpPost("Add")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Add([FromBody] clsActionDto dto)
    {
        var result = await _actionService.AddAsync(dto.ActionName);

        switch (result)
        {
            case enOperationResult.Success:
                return StatusCode(
                    StatusCodes.Status201Created,
                    new { message = "Action added successfully." });

            case enOperationResult.InvalidData:
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid Data",
                    detail: "ActionName is required and must not exceed 50 characters.");

            case enOperationResult.Conflict:
                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Conflict",
                    detail: "An Action with this name already exists.");

            default:
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal Server Error",
                    detail: "An error occurred while adding the Action.");
        }
    }
    [Authorize]
    [HttpPut("Update/{actionId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        int actionId,
        [FromBody] clsActionDto dto)
    {
        var result = await _actionService.UpdateAsync(
            actionId,
            dto.ActionName);

        switch (result)
        {
            case enOperationResult.Success:
                return Ok(new { message = "Action updated successfully." });

            case enOperationResult.InvalidData:
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid Data",
                    detail: "ActionName is required and must not exceed 50 characters.");

            case enOperationResult.NotFound:
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    detail: "Action was not found.");

            case enOperationResult.Conflict:
                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Conflict",
                    detail: "Another Action with this name already exists.");

            default:
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal Server Error",
                    detail: "An error occurred while updating the Action.");
        }
    }

    [Authorize]
    [HttpDelete("Delete/{actionId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int actionId)
    {
        var result = await _actionService.DeleteAsync(actionId);

        switch (result)
        {
            case enOperationResult.Success:
                return Ok(new
                {
                    message = "Action and its related audit logs deleted successfully."
                });

            case enOperationResult.NotFound:
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    detail: "Action was not found.");

            default:
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal Server Error",
                    detail: "An error occurred while deleting the Action.");
        }
    }
}