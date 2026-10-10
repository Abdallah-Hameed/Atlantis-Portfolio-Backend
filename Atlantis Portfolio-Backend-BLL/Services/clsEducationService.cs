using Atlantis_Portfolio_Backend_DAL.Context;
using Atlantis_Portfolio_Backend_DAL.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlantis_Portfolio_Backend_BLL;

public class clsEducationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<clsEducationService> _logger;

    public clsEducationService(
        AppDbContext context,
        ILogger<clsEducationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Education>> GetAllEducationsAsync()
    {
        try
        {
            return await _context.Educations
                .AsNoTracking()
                .OrderBy(e => e.DisplayOrder)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all educations.");
            throw;
        }
    }

    public async Task<Education?> GetEducationByIdAsync(int educationId)
    {
        if (educationId <= 0)
            return null;

        try
        {
            return await _context.Educations
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EducationId == educationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while retrieving education. EducationId: {EducationId}",
                educationId);

            throw;
        }
    }

    public async Task<enOperationResult> AddEducationAsync(
        string institution,
        string degree,
        string fieldOfStudy,
        string? description,
        DateOnly startDate,
        DateOnly? endDate,
        bool isCurrent)
    {
        if (!IsValidEducationData(
                institution, degree, fieldOfStudy,
                startDate, endDate, isCurrent))
        {
            return enOperationResult.InvalidData;
        }

        institution = institution.Trim();
        degree = degree.Trim();
        fieldOfStudy = fieldOfStudy.Trim();
        description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        try
        {
            short? maxDisplayOrder = await _context.Educations
                .MaxAsync(e => (short?)e.DisplayOrder);

            if (maxDisplayOrder == short.MaxValue)
            {
                _logger.LogWarning(
                    "Cannot add education because the maximum DisplayOrder has been reached.");

                return enOperationResult.Failed;
            }

            var education = new Education
            {
                Institution = institution,
                Degree = degree,
                FieldOfStudy = fieldOfStudy,
                Description = description,
                StartDate = startDate,
                EndDate = endDate,
                IsCurrent = isCurrent,
                DisplayOrder = (short)((maxDisplayOrder ?? 0) + 1)
            };

            await _context.Educations.AddAsync(education);

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation(
                    "Education added successfully. EducationId: {EducationId}",
                    education.EducationId);

                return enOperationResult.Success;
            }

            _logger.LogWarning("Education add operation failed.");

            return enOperationResult.Failed;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlEx &&
                  (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _logger.LogWarning(
                "Cannot add education because a display order conflict occurred.");

            return enOperationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding education.");

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> UpdateEducationAsync(
        int educationId,
        string institution,
        string degree,
        string fieldOfStudy,
        string? description,
        DateOnly startDate,
        DateOnly? endDate,
        bool isCurrent)
    {
        if (educationId <= 0 ||
            !IsValidEducationData(
                institution, degree, fieldOfStudy,
                startDate, endDate, isCurrent))
        {
            return enOperationResult.InvalidData;
        }

        institution = institution.Trim();
        degree = degree.Trim();
        fieldOfStudy = fieldOfStudy.Trim();
        description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        try
        {
            var education = await _context.Educations.FindAsync(educationId);

            if (education == null)
            {
                _logger.LogWarning(
                    "Education update failed. EducationId: {EducationId}",
                    educationId);

                return enOperationResult.NotFound;
            }

            if (education.Institution == institution &&
                education.Degree == degree &&
                education.FieldOfStudy == fieldOfStudy &&
                education.Description == description &&
                education.StartDate == startDate &&
                education.EndDate == endDate &&
                education.IsCurrent == isCurrent)
            {
                _logger.LogInformation(
                    "Education update skipped because no changes were detected. EducationId: {EducationId}",
                    educationId);

                return enOperationResult.Success;
            }

            education.Institution = institution;
            education.Degree = degree;
            education.FieldOfStudy = fieldOfStudy;
            education.Description = description;
            education.StartDate = startDate;
            education.EndDate = endDate;
            education.IsCurrent = isCurrent;

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation(
                    "Education updated successfully. EducationId: {EducationId}",
                    educationId);

                return enOperationResult.Success;
            }

            _logger.LogWarning(
                "Education update operation failed. EducationId: {EducationId}",
                educationId);

            return enOperationResult.Failed;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlEx &&
                  (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _logger.LogWarning(
                "Education update encountered a unique constraint conflict. EducationId: {EducationId}",
                educationId);

            return enOperationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while updating education. EducationId: {EducationId}",
                educationId);

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> DeleteEducationAsync(int educationId)
    {
        if (educationId <= 0)
            return enOperationResult.InvalidData;

        try
        {
            var education = await _context.Educations.FindAsync(educationId);

            if (education == null)
            {
                _logger.LogWarning(
                    "Education deletion failed. EducationId: {EducationId}",
                    educationId);

                return enOperationResult.NotFound;
            }

            _context.Educations.Remove(education);

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation(
                    "Education deleted successfully. EducationId: {EducationId}",
                    educationId);

                return enOperationResult.Success;
            }

            _logger.LogWarning(
                "Education deletion operation failed. EducationId: {EducationId}",
                educationId);

            return enOperationResult.Failed;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlEx &&
                  sqlEx.Number == 547)
        {
            _logger.LogWarning(
                "Education deletion was blocked by a database constraint. EducationId: {EducationId}",
                educationId);

            return enOperationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while deleting education. EducationId: {EducationId}",
                educationId);

            return enOperationResult.Failed;
        }
    }

    private static bool IsValidEducationData(
        string institution,
        string degree,
        string fieldOfStudy,
        DateOnly startDate,
        DateOnly? endDate,
        bool isCurrent)
    {
        if (string.IsNullOrWhiteSpace(institution) ||
            institution.Trim().Length > 200 ||
            string.IsNullOrWhiteSpace(degree) ||
            degree.Trim().Length > 150 ||
            string.IsNullOrWhiteSpace(fieldOfStudy) ||
            fieldOfStudy.Trim().Length > 200)
        {
            return false;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (startDate == default || startDate > today)
            return false;

        if (isCurrent)
            return endDate == null;

        return endDate.HasValue &&
               endDate.Value >= startDate &&
               endDate.Value <= today;
    }
}