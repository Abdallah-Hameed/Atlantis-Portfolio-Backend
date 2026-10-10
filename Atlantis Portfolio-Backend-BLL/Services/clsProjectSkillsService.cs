using Atlantis_Portfolio_Backend_DAL.Context;
using Atlantis_Portfolio_Backend_DAL.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlantis_Portfolio_Backend_BLL;

public class clsProjectSkillService
{
    private readonly AppDbContext _context;
    private readonly ILogger<clsProjectSkillService> _logger;

    public clsProjectSkillService(AppDbContext context, ILogger<clsProjectSkillService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ProjectSkill>> GetProjectSkillsByIDAsync(int projectId)
    {
        if (projectId <= 0)
            return new List<ProjectSkill>();

        try
        {
            return await _context.ProjectSkills.AsNoTracking().Where(ps => ps.ProjectId == projectId).Include(ps => ps.Project).Include(ps => ps.Skill).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving skills for project. ProjectId: {ProjectId}", projectId);

            throw;
        }
    }

    public async Task<List<ProjectSkill>> GetAllProjectSkillsAsync()
    {
        try
        {
            return await _context.ProjectSkills.AsNoTracking().Include(ps => ps.Project).Include(ps => ps.Skill).OrderBy(ps => ps.ProjectId).ThenBy(ps => ps.SkillId).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all project skills.");

            throw;
        }
    }

    public async Task<enOperationResult> AddProjectSkillAsync(int projectId, int skillId)
    {
        if (projectId <= 0 || skillId <= 0)
            return enOperationResult.InvalidData;

        try
        {
            var projectExists = await _context.Projects.AnyAsync(p => p.ProjectId == projectId);

            if (!projectExists)
            {
                _logger.LogWarning("Adding skill to project failed because the project was not found. ProjectId: {ProjectId}", projectId);

                return enOperationResult.NotFound;
            }

            var skillExists = await _context.Skills.AnyAsync(s => s.SkillId == skillId);

            if (!skillExists)
            {
                _logger.LogWarning("Adding skill to project failed because the skill was not found. SkillId: {SkillId}", skillId);

                return enOperationResult.NotFound;
            }

            var projectSkillExists = await _context.ProjectSkills.AnyAsync(ps => ps.ProjectId == projectId && ps.SkillId == skillId);

            if (projectSkillExists)
            {
                _logger.LogWarning("Adding skill to project failed because the association already exists. ProjectId: {ProjectId}, SkillId: {SkillId}", projectId, skillId);

                return enOperationResult.AlreadyExist;
            }

            var projectSkill = new ProjectSkill
            {
                ProjectId = projectId,
                SkillId = skillId
            };

            await _context.ProjectSkills.AddAsync(projectSkill);

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation("Skill added to project successfully. ProjectSkillId: {ProjectSkillId}, ProjectId: {ProjectId}, SkillId: {SkillId}", projectSkill.ProjectSkillId, projectId, skillId);

                return enOperationResult.Success;
            }

            _logger.LogWarning("Adding skill to project failed. ProjectId: {ProjectId}, SkillId: {SkillId}", projectId, skillId);

            return enOperationResult.Failed;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _logger.LogWarning("Adding skill to project failed because the association already exists. ProjectId: {ProjectId}, SkillId: {SkillId}", projectId, skillId);

            return enOperationResult.AlreadyExist;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547)
        {
            _logger.LogWarning("Adding skill to project failed because a referenced project or skill does not exist. ProjectId: {ProjectId}, SkillId: {SkillId}", projectId, skillId);

            return enOperationResult.NotFound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding skill to project. ProjectId: {ProjectId}, SkillId: {SkillId}", projectId, skillId);

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> DeleteProjectSkillAsync(int projectSkillId)
    {
        if (projectSkillId <= 0)
            return enOperationResult.InvalidData;

        try
        {
            var projectSkill = await _context.ProjectSkills.FindAsync(projectSkillId);

            if (projectSkill == null)
            {
                _logger.LogWarning("Project skill deletion failed. ProjectSkillId: {ProjectSkillId}", projectSkillId);

                return enOperationResult.NotFound;
            }

            _context.ProjectSkills.Remove(projectSkill);

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation("Skill removed from project successfully. ProjectSkillId: {ProjectSkillId}, ProjectId: {ProjectId}, SkillId: {SkillId}", projectSkillId, projectSkill.ProjectId, projectSkill.SkillId);

                return enOperationResult.Success;
            }

            _logger.LogWarning("Removing skill from project failed. ProjectSkillId: {ProjectSkillId}", projectSkillId);

            return enOperationResult.Failed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting project skill. ProjectSkillId: {ProjectSkillId}", projectSkillId);

            return enOperationResult.Failed;
        }
    }
}