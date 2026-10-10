using System.Security.Claims;
using Atlantis_Portfolio_Backend_BLL;
using Atlantis_Portfolio_Backend.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlantis_Portfolio_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly clsUserService _userService;

    public UserController(clsUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet("Get/{userId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);

        if (user == null)
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "User not found", detail: $"User with ID {userId} not found.");

        var response = new clsUserResponseDto
        {
            UserId = user.UserId,
            Email = user.Email,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };

        return Ok(response);
    }

    [Authorize]
    [HttpPost("Add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUser([FromBody] clsCreateUserRequestDto request)
    {
        var result = await _userService.AddUserAsync(request.Email, request.Password);

        return result switch
        {
            enOperationResult.Success =>
                Ok("User created successfully."),

            enOperationResult.AlreadyExist =>
                Problem(statusCode: StatusCodes.Status409Conflict, title: "User already exists", detail: "The email address is already in use."),

            enOperationResult.InvalidData =>
                Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid user data", detail: "The provided user data is invalid."),

            _ =>
                Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", detail: "Failed to create user.")
        };
    }

    [Authorize]
    [HttpPut("Update/Email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeEmail([FromBody] clsChangeEmailDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid token.");

        var result = await _userService.ChangeEmailAsync(userId, request.NewEmail);

        return result switch
        {
            enOperationResult.Success =>
                Ok("Email changed successfully."),

            enOperationResult.InvalidData =>
                Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid email data", detail: "The provided email data is invalid."),

            enOperationResult.NotFound =>
                Problem(statusCode: StatusCodes.Status404NotFound, title: "User not found", detail: "User not found."),

            enOperationResult.AlreadyExist =>
                Problem(statusCode: StatusCodes.Status409Conflict, title: "Email already exists", detail: "The email address is already in use."),

            _ =>
                Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", detail: "Failed to change email.")
        };
    }

    [Authorize]
    [HttpPut("Update/Password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangePassword([FromBody] clsChangePasswordDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid token.");

        var result = await _userService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

        return result switch
        {
            enOperationResult.Success =>
                Ok("Password changed successfully."),

            enOperationResult.InvalidData =>
                Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid password data", detail: "The provided password data is invalid."),

            enOperationResult.NotFound =>
                Problem(statusCode: StatusCodes.Status404NotFound, title: "User not found", detail: "User not found."),

            enOperationResult.InvalidCredentials =>
                Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid credentials", detail: "Current password is incorrect."),

            _ =>
                Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", detail: "Failed to change password.")
        };
    }

    [Authorize]
    [HttpPut("Update/Status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetUserActive([FromBody] clsSetUserActiveDto request)
    {
        var result = await _userService.SetUserActiveAsync(request.UserId, request.IsActive);

        return result switch
        {
            enOperationResult.Success =>
                Ok("User status updated successfully."),

            enOperationResult.InvalidData =>
                Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid user data", detail: "The provided user data is invalid."),

            enOperationResult.NotFound =>
                Problem(statusCode: StatusCodes.Status404NotFound, title: "User not found", detail: $"User with ID {request.UserId} not found."),

            _ =>
                Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", detail: "Failed to update user status.")
        };
    }
}