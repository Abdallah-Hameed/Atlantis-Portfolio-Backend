using Atlantis_Portfolio_Backend_DAL.Context;
using Atlantis_Portfolio_Backend_DAL.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlantis_Portfolio_Backend_BLL;

public class clsCourseService
{
    private readonly AppDbContext _context;
    private readonly ILogger<clsCourseService> _logger;

    public clsCourseService(
        AppDbContext context,
        ILogger<clsCourseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Course>> GetAllCoursesAsync()
    {
        try
        {
            return await _context.Courses
                .AsNoTracking()
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all courses.");
            throw;
        }
    }

    public async Task<Course?> GetCourseByIdAsync(int courseId)
    {
        if (courseId <= 0)
            return null;

        try
        {
            return await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while retrieving course. CourseId: {CourseId}",
                courseId);

            throw;
        }
    }

    public async Task<enOperationResult> AddCourseAsync(
        string courseName,
        string provider,
        string? description,
        DateOnly? completionDate,
        string? certificateUrl)
    {
        if (!IsValidCourseData(courseName, provider, certificateUrl))
            return enOperationResult.InvalidData;

        courseName = courseName.Trim();
        provider = provider.Trim();
        description = NormalizeOptionalText(description);
        certificateUrl = NormalizeOptionalText(certificateUrl);

        try
        {
            short? maxDisplayOrder = await _context.Courses
                .MaxAsync(c => (short?)c.DisplayOrder);

            if (maxDisplayOrder == short.MaxValue)
            {
                _logger.LogWarning(
                    "Cannot add course because the maximum DisplayOrder has been reached.");

                return enOperationResult.Failed;
            }

            var course = new Course
            {
                CourseName = courseName,
                Provider = provider,
                Description = description,
                CompletionDate = completionDate,
                CertificateUrl = certificateUrl,
                DisplayOrder = (short)((maxDisplayOrder ?? 0) + 1)
            };

            await _context.Courses.AddAsync(course);

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation(
                    "Course added successfully. CourseId: {CourseId}",
                    course.CourseId);

                return enOperationResult.Success;
            }

            _logger.LogWarning("Course add operation failed.");

            return enOperationResult.Failed;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlEx &&
                  (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _logger.LogWarning(
                "Cannot add course because a display order conflict occurred.");

            return enOperationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding course.");

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> UpdateCourseAsync(
        int courseId,
        string courseName,
        string provider,
        string? description,
        DateOnly? completionDate,
        string? certificateUrl)
    {
        if (courseId <= 0 ||
            !IsValidCourseData(courseName, provider, certificateUrl))
        {
            return enOperationResult.InvalidData;
        }

        courseName = courseName.Trim();
        provider = provider.Trim();
        description = NormalizeOptionalText(description);
        certificateUrl = NormalizeOptionalText(certificateUrl);

        try
        {
            var course = await _context.Courses.FindAsync(courseId);

            if (course == null)
            {
                _logger.LogWarning(
                    "Course update failed. CourseId: {CourseId}",
                    courseId);

                return enOperationResult.NotFound;
            }

            if (course.CourseName == courseName &&
                course.Provider == provider &&
                course.Description == description &&
                course.CompletionDate == completionDate &&
                course.CertificateUrl == certificateUrl)
            {
                _logger.LogInformation(
                    "Course update skipped because no changes were detected. CourseId: {CourseId}",
                    courseId);

                return enOperationResult.Success;
            }

            course.CourseName = courseName;
            course.Provider = provider;
            course.Description = description;
            course.CompletionDate = completionDate;
            course.CertificateUrl = certificateUrl;

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation(
                    "Course updated successfully. CourseId: {CourseId}",
                    courseId);

                return enOperationResult.Success;
            }

            _logger.LogWarning(
                "Course update operation failed. CourseId: {CourseId}",
                courseId);

            return enOperationResult.Failed;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlEx &&
                  (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _logger.LogWarning(
                "Course update encountered a unique constraint conflict. CourseId: {CourseId}",
                courseId);

            return enOperationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while updating course. CourseId: {CourseId}",
                courseId);

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> DeleteCourseAsync(int courseId)
    {
        if (courseId <= 0)
            return enOperationResult.InvalidData;

        try
        {
            var course = await _context.Courses.FindAsync(courseId);

            if (course == null)
            {
                _logger.LogWarning(
                    "Course deletion failed. CourseId: {CourseId}",
                    courseId);

                return enOperationResult.NotFound;
            }

            _context.Courses.Remove(course);

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation(
                    "Course deleted successfully. CourseId: {CourseId}",
                    courseId);

                return enOperationResult.Success;
            }

            _logger.LogWarning(
                "Course deletion operation failed. CourseId: {CourseId}",
                courseId);

            return enOperationResult.Failed;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlEx &&
                  sqlEx.Number == 547)
        {
            _logger.LogWarning(
                "Course deletion was blocked by a database constraint. CourseId: {CourseId}",
                courseId);

            return enOperationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while deleting course. CourseId: {CourseId}",
                courseId);

            return enOperationResult.Failed;
        }
    }

    private static bool IsValidCourseData(
        string courseName,
        string provider,
        string? certificateUrl)
    {
        if (string.IsNullOrWhiteSpace(courseName) ||
            courseName.Trim().Length > 200 ||
            string.IsNullOrWhiteSpace(provider) ||
            provider.Trim().Length > 150)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(certificateUrl))
        {
            if (certificateUrl.Trim().Length > 500 ||
                !Uri.TryCreate(certificateUrl.Trim(), UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp &&
                 uri.Scheme != Uri.UriSchemeHttps))
            {
                return false;
            }
        }

        return true;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}