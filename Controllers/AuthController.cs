using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Atlantis_Portfolio_Backend.DTOs.Auth;
using Atlantis_Portfolio_Backend_BLL;

namespace Atlantis_Portfolio_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly clsAuthService _authService;

    public AuthController(clsAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("Login")]
    [EnableRateLimiting("AuthRateLimitPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] clsLoginRequestDto request)
    {
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var loginResult = await _authService.LoginAsync(
            request.Email,
            request.Password,
            ipAddress);

        if (loginResult.Result == enOperationResult.Success)
        {
            return Ok(new
            {
                AccessToken = loginResult.AccessToken,
                RefreshToken = loginResult.RefreshToken
            });
        }

        return loginResult.Result switch
        {
            enOperationResult.InvalidData =>
                Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Invalid credentials",
                    detail: "The email or password is incorrect."),

            enOperationResult.AccountInactive =>
                Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Invalid credentials",
                    detail: "The email or password is incorrect."),

            enOperationResult.AccountLocked =>
                Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Invalid credentials",
                    detail: "The email or password is incorrect."),

            enOperationResult.InvalidCredentials =>
                Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Invalid credentials",
                    detail: "The email or password is incorrect."),

            _ =>
                Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal server error",
                    detail: "Failed to process login.")
        };
    }

    [HttpPost("Refresh")]
    [EnableRateLimiting("AuthRateLimitPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Refresh([FromBody] clsRefreshRequestDto request)
    {
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var refreshResult = await _authService.RefreshAsync(
            request.RefreshToken,
            ipAddress);

        if (refreshResult.Result == enOperationResult.Success)
        {
            return Ok(new
            {
                AccessToken = refreshResult.AccessToken,
                RefreshToken = refreshResult.RefreshToken
            });
        }

        return refreshResult.Result switch
        {
            enOperationResult.InvalidData =>
                Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid request",
                    detail: "The refresh token is required."),

            enOperationResult.AccountInactive =>
                Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Invalid refresh token",
                    detail: "The refresh token is invalid, revoked, or expired."),

            enOperationResult.InvalidCredentials =>
                Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Invalid refresh token",
                    detail: "The refresh token is invalid, revoked, or expired."),

            _ =>
                Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal server error",
                    detail: "Failed to refresh token.")
        };
    }

    [HttpPost("Logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout([FromBody] clsRefreshRequestDto request)
    {
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var logoutResult = await _authService.LogoutAsync(
            request.RefreshToken,
            ipAddress);

        if (logoutResult == enOperationResult.Success)
        {
            return Ok();
        }

        return logoutResult switch
        {
            enOperationResult.InvalidData =>
                Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid request",
                    detail: "The refresh token is required."),

            enOperationResult.InvalidCredentials =>
                Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Invalid refresh token",
                    detail: "The refresh token is invalid or already revoked."),

            _ =>
                Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal server error",
                    detail: "Failed to logout.")
        };
    }
}