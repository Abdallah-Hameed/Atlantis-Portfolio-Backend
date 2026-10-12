using Atlantis_Portfolio_Backend_DAL.Context;
using Atlantis_Portfolio_Backend_DAL.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlantis_Portfolio_Backend_BLL;

public class clsProjectService
{
    private readonly AppDbContext _context;
    private readonly ILogger<clsProjectService> _logger;

    public clsProjectService(AppDbContext context, ILogger<clsProjectService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Project>> GetAllProjectsAsync()
    {
        try
        {
            return await _context.Projects.AsNoTracking().OrderBy(p => p.DisplayOrder).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all projects.");

            throw;
        }
    }

    public async Task<Project?> GetProjectByIdAsync(int projectId)
    {
        if (projectId <= 0)
            return null;

        try
        {
            return await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.ProjectId == projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving project. ProjectId: {ProjectId}", projectId);

            throw;
        }
    }

    public async Task<enOperationResult> AddProjectAsync(string title, string description, string? imageUrl, string? gitHubUrl, string? liveUrl)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
            return enOperationResult.InvalidData;

        title = title.Trim();
        description = description.Trim();
        imageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        gitHubUrl = string.IsNullOrWhiteSpace(gitHubUrl) ? null : gitHubUrl.Trim();
        liveUrl = string.IsNullOrWhiteSpace(liveUrl) ? null : liveUrl.Trim();

        try
        {
            short? maxDisplayOrder = await _context.Projects.MaxAsync(p => (short?)p.DisplayOrder);

            if (maxDisplayOrder == short.MaxValue)
            {
                _logger.LogWarning("Cannot add project because the maximum DisplayOrder has been reached.");

                return enOperationResult.Failed;
            }

            var project = new Project
            {
                Title = title,
                Description = description,
                ImageUrl = imageUrl,
                GitHubUrl = gitHubUrl,
                LiveUrl = liveUrl,
                DisplayOrder = (short)((maxDisplayOrder ?? 0) + 1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Projects.AddAsync(project);

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation("Project added successfully. ProjectId: {ProjectId}", project.ProjectId);

                return enOperationResult.Success;
            }

            _logger.LogWarning("Project add operation failed.");

            return enOperationResult.Failed;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _logger.LogWarning("Cannot add project because a display order conflict occurred.");

            return enOperationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding project.");

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> UpdateProjectAsync(int projectId, string title, string description, string? imageUrl, string? gitHubUrl, string? liveUrl)
    {
        if (projectId <= 0 || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
            return enOperationResult.InvalidData;

        title = title.Trim();
        description = description.Trim();
        imageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        gitHubUrl = string.IsNullOrWhiteSpace(gitHubUrl) ? null : gitHubUrl.Trim();
        liveUrl = string.IsNullOrWhiteSpace(liveUrl) ? null : liveUrl.Trim();

        try
        {
            var project = await _context.Projects.FindAsync(projectId);

            if (project == null)
            {
                _logger.LogWarning("Project update failed. ProjectId: {ProjectId}", projectId);

                return enOperationResult.NotFound;
            }

            if (project.Title == title && project.Description == description && project.ImageUrl == imageUrl && project.GitHubUrl == gitHubUrl && project.LiveUrl == liveUrl)
            {
                _logger.LogInformation("Project update skipped because no changes were detected. ProjectId: {ProjectId}", projectId);

                return enOperationResult.Success;
            }

            project.Title = title;
            project.Description = description;
            project.ImageUrl = imageUrl;
            project.GitHubUrl = gitHubUrl;
            project.LiveUrl = liveUrl;
            project.UpdatedAt = DateTime.UtcNow;

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation("Project updated successfully. ProjectId: {ProjectId}", projectId);

                return enOperationResult.Success;
            }

            _logger.LogWarning("Project update operation failed. ProjectId: {ProjectId}", projectId);

            return enOperationResult.Failed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating project. ProjectId: {ProjectId}", projectId);

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> DeleteProjectAsync(int projectId)
    {
        if (projectId <= 0)
            return enOperationResult.InvalidData;

        try
        {
            var project = await _context.Projects.FindAsync(projectId);

            if (project == null)
            {
                _logger.LogWarning("Project deletion failed. ProjectId: {ProjectId}", projectId);

                return enOperationResult.NotFound;
            }

            var projectSkills = await _context.ProjectSkills.Where(ps => ps.ProjectId == projectId).ToListAsync();

            if (projectSkills.Count > 0)
                _context.ProjectSkills.RemoveRange(projectSkills);

            _context.Projects.Remove(project);

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation("Project deleted successfully. ProjectId: {ProjectId}", projectId);

                return enOperationResult.Success;
            }

            _logger.LogWarning("Project deletion operation failed. ProjectId: {ProjectId}", projectId);

            return enOperationResult.Failed;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547)
        {
            _logger.LogWarning("Project deletion was blocked because related records still reference it. ProjectId: {ProjectId}", projectId);

            return enOperationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting project. ProjectId: {ProjectId}", projectId);

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> ReorderProjectsAsync(List<(int ProjectId, short DisplayOrder)> items)
    {
        if (items == null || items.Count == 0)
            return enOperationResult.InvalidData;

        if (items.Any(x => x.ProjectId <= 0 || x.DisplayOrder < 1))
            return enOperationResult.InvalidData;

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var ids = items.Select(x => x.ProjectId).ToList();
            var projects = await _context.Projects
                .Where(p => ids.Contains(p.ProjectId))
                .ToListAsync();

            if (projects.Count != ids.Count)
                return enOperationResult.NotFound;

            // Phase 1: Offset to safe temporary values
            foreach (var project in projects)
            {
                project.DisplayOrder += 10000;
            }
            await _context.SaveChangesAsync();

            // Phase 2: Set final values
            var dict = items.ToDictionary(x => x.ProjectId, x => x.DisplayOrder);
            foreach (var project in projects)
            {
                project.DisplayOrder = dict[project.ProjectId];
            }
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogInformation("Projects reordered successfully. Count: {Count}", items.Count);

            return enOperationResult.Success;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _logger.LogWarning("Project reorder failed due to a unique constraint violation.");

            return enOperationResult.AlreadyExist;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reordering projects.");

            return enOperationResult.Failed;
        }
    }
}