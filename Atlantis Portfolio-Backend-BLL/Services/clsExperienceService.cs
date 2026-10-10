using Atlantis_Portfolio_Backend_DAL.Context;
using Atlantis_Portfolio_Backend_DAL.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlantis_Portfolio_Backend_BLL;

public class clsExperienceService
{
    private readonly AppDbContext _context;
    private readonly ILogger<clsExperienceService> _logger;

    public clsExperienceService(
        AppDbContext context,
        ILogger<clsExperienceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Experience>> GetAllExperiencesAsync()
    {
        try
        {
            return await _context.Experiences
                .AsNoTracking()
                .OrderBy(e => e.DisplayOrder)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all experiences.");
            throw;
        }
    }

    public async Task<Experience?> GetExperienceByIdAsync(int experienceId)
    {
        if (experienceId <= 0)
            return null;

        try
        {
            return await _context.Experiences
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ExperienceId == experienceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while retrieving experience. ExperienceId: {ExperienceId}",
                experienceId);

            throw;
        }
    }

    public async Task<enOperationResult> AddExperienceAsync(
        string company,
        string position,
        string description,
        DateOnly startDate,
        DateOnly? endDate,
        bool isCurrent)
    {
        if (!IsValidExperienceData(
                company, position, description, startDate, endDate, isCurrent))
        {
            return enOperationResult.InvalidData;
        }

        company = company.Trim();
        position = position.Trim();
        description = description.Trim();

        try
        {
            short? maxDisplayOrder = await _context.Experiences
                .MaxAsync(e => (short?)e.DisplayOrder);

            if (maxDisplayOrder == short.MaxValue)
            {
                _logger.LogWarning(
                    "Cannot add experience because the maximum DisplayOrder has been reached.");

                return enOperationResult.Failed;
            }

            var experience = new Experience
            {
                Company = company,
                Position = position,
                Description = description,
                StartDate = startDate,
                EndDate = endDate,
                IsCurrent = isCurrent,
                DisplayOrder = (short)((maxDisplayOrder ?? 0) + 1)
            };

            await _context.Experiences.AddAsync(experience);

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation(
                    "Experience added successfully. ExperienceId: {ExperienceId}",
                    experience.ExperienceId);

                return enOperationResult.Success;
            }

            _logger.LogWarning("Experience add operation failed.");
            return enOperationResult.Failed;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlEx &&
                  (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _logger.LogWarning(
                "Cannot add experience because a display order conflict occurred.");

            return enOperationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding experience.");
            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> UpdateExperienceAsync(
        int experienceId,
        string company,
        string position,
        string description,
        DateOnly startDate,
        DateOnly? endDate,
        bool isCurrent)
    {
        if (experienceId <= 0 ||
            !IsValidExperienceData(
                company, position, description, startDate, endDate, isCurrent))
        {
            return enOperationResult.InvalidData;
        }

        company = company.Trim();
        position = position.Trim();
        description = description.Trim();

        try
        {
            var experience = await _context.Experiences.FindAsync(experienceId);

            if (experience == null)
            {
                _logger.LogWarning(
                    "Experience update failed. ExperienceId: {ExperienceId}",
                    experienceId);

                return enOperationResult.NotFound;
            }

            if (experience.Company == company &&
                experience.Position == position &&
                experience.Description == description &&
                experience.StartDate == startDate &&
                experience.EndDate == endDate &&
                experience.IsCurrent == isCurrent)
            {
                _logger.LogInformation(
                    "Experience update skipped because no changes were detected. ExperienceId: {ExperienceId}",
                    experienceId);

                return enOperationResult.Success;
            }

            experience.Company = company;
            experience.Position = position;
            experience.Description = description;
            experience.StartDate = startDate;
            experience.EndDate = endDate;
            experience.IsCurrent = isCurrent;

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation(
                    "Experience updated successfully. ExperienceId: {ExperienceId}",
                    experienceId);

                return enOperationResult.Success;
            }

            _logger.LogWarning(
                "Experience update operation failed. ExperienceId: {ExperienceId}",
                experienceId);

            return enOperationResult.Failed;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlEx &&
                  (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _logger.LogWarning(
                "Experience update encountered a unique constraint conflict. ExperienceId: {ExperienceId}",
                experienceId);

            return enOperationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while updating experience. ExperienceId: {ExperienceId}",
                experienceId);

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> DeleteExperienceAsync(int experienceId)
    {
        if (experienceId <= 0)
            return enOperationResult.InvalidData;

        try
        {
            var experience = await _context.Experiences.FindAsync(experienceId);

            if (experience == null)
            {
                _logger.LogWarning(
                    "Experience deletion failed. ExperienceId: {ExperienceId}",
                    experienceId);

                return enOperationResult.NotFound;
            }

            _context.Experiences.Remove(experience);

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation(
                    "Experience deleted successfully. ExperienceId: {ExperienceId}",
                    experienceId);

                return enOperationResult.Success;
            }

            _logger.LogWarning(
                "Experience deletion operation failed. ExperienceId: {ExperienceId}",
                experienceId);

            return enOperationResult.Failed;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547)
        {
            _logger.LogWarning(
                "Experience deletion was blocked by a database constraint. ExperienceId: {ExperienceId}",
                experienceId);

            return enOperationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while deleting experience. ExperienceId: {ExperienceId}",
                experienceId);

            return enOperationResult.Failed;
        }
    }

    private static bool IsValidExperienceData(
        string company,
        string position,
        string description,
        DateOnly startDate,
        DateOnly? endDate,
        bool isCurrent)
    {
        if (string.IsNullOrWhiteSpace(company) ||
            company.Trim().Length > 200 ||
            string.IsNullOrWhiteSpace(position) ||
            position.Trim().Length > 200 ||
            string.IsNullOrWhiteSpace(description))
        {
            return false;
        }

        if (startDate == default || startDate > DateOnly.FromDateTime(DateTime.UtcNow))
            return false;

        if (isCurrent)
            return endDate == null;

        return endDate.HasValue &&
               endDate.Value >= startDate &&
               endDate.Value <= DateOnly.FromDateTime(DateTime.UtcNow);
    }
}