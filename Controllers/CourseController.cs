using Atlantis_Portfolio_Backend_BLL;
using Atlantis_Portfolio_Backend.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlantis_Portfolio_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourseController : ControllerBase
{
    private readonly clsCourseService _courseService;

    public CourseController(clsCourseService courseService)
    {
        _courseService = courseService;
    }

    [AllowAnonymous]
    [HttpGet("Get/All")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCourses()
    {
        var courses = await _courseService.GetAllCoursesAsync();

        return Ok(courses);
    }

    [Authorize]
    [HttpGet("Get/{courseId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseById(int courseId)
    {
        var course = await _courseService.GetCourseByIdAsync(courseId);

        if (course == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Course not found",
                detail: $"Course with ID {courseId} not found.");
        }

        return Ok(course);
    }

    [Authorize]
    [HttpPost("Add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddCourse([FromBody] clsAddCourseDto dto)
    {
        var result = await _courseService.AddCourseAsync(
            dto.CourseName,
            dto.Provider,
            dto.Description,
            dto.CompletionDate,
            dto.CertificateUrl);

        return result switch
        {
            enOperationResult.Success =>
                Ok("Course added successfully."),

            enOperationResult.InvalidData =>
                Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid course data",
                    detail: "The provided course data is invalid."),

            enOperationResult.Conflict =>
                Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Course conflict",
                    detail: "A conflict occurred while saving the course. Please try again."),

            _ =>
                Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal server error",
                    detail: "Failed to add course.")
        };
    }

    [Authorize]
    [HttpPut("Update/{courseId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCourse(
        int courseId,
        [FromBody] clsUpdateCourseDto dto)
    {
        var result = await _courseService.UpdateCourseAsync(
            courseId,
            dto.CourseName,
            dto.Provider,
            dto.Description,
            dto.CompletionDate,
            dto.CertificateUrl);

        return result switch
        {
            enOperationResult.Success =>
                Ok("Course updated successfully."),

            enOperationResult.InvalidData =>
                Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid course data",
                    detail: "The provided course data is invalid."),

            enOperationResult.NotFound =>
                Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Course not found",
                    detail: $"Course with ID {courseId} not found."),

            _ =>
                Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal server error",
                    detail: "Failed to update course.")
        };
    }

    [Authorize]
    [HttpDelete("Delete/{courseId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCourse(int courseId)
    {
        var result = await _courseService.DeleteCourseAsync(courseId);

        return result switch
        {
            enOperationResult.Success =>
                Ok("Course deleted successfully."),

            enOperationResult.InvalidData =>
                Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid course ID",
                    detail: "Course ID must be greater than zero."),

            enOperationResult.NotFound =>
                Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Course not found",
                    detail: $"Course with ID {courseId} not found."),

            enOperationResult.Conflict =>
                Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Course deletion conflict",
                    detail: "The course could not be deleted because of a conflict with related records."),

            _ =>
                Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal server error",
                    detail: "Failed to delete course.")
        };
    }
}