using Atlantis_Portfolio_Backend_DAL.Context;
using Atlantis_Portfolio_Backend_DAL.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlantis_Portfolio_Backend_BLL;

public class clsSkillService
{
    private readonly AppDbContext _context;
    private readonly ILogger<clsSkillService> _logger;

    public clsSkillService(AppDbContext context, ILogger<clsSkillService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Skill>> GetAllSkillsAsync()
    {
        try
        {
            return await _context.Skills.AsNoTracking().OrderBy(s => s.DisplayOrder).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all skills.");

            throw;
        }
    }

    public async Task<Skill?> GetSkillByIdAsync(int skillId)
    {
        if (skillId <= 0)
            return null;

        try
        {
            return await _context.Skills.AsNoTracking().FirstOrDefaultAsync(s => s.SkillId == skillId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving skill. SkillId: {SkillId}", skillId);

            throw;
        }
    }

    public async Task<enOperationResult> AddSkillAsync(string skillName, string skillCategory, byte skillLevel)
    {
        if (string.IsNullOrWhiteSpace(skillName) || string.IsNullOrWhiteSpace(skillCategory))
            return enOperationResult.InvalidData;

        if (skillLevel is < 1 or > 5)
            return enOperationResult.InvalidSkillLevel;

        skillName = skillName.Trim();
        skillCategory = skillCategory.Trim();

        try
        {
            short? maxDisplayOrder = await _context.Skills.Where(s => s.SkillCategory == skillCategory).MaxAsync(s => (short?)s.DisplayOrder);

            if (maxDisplayOrder == short.MaxValue)
            {
                _logger.LogWarning("Cannot add skill because the maximum DisplayOrder has been reached for category {SkillCategory}.", skillCategory);

                return enOperationResult.Failed;
            }

            short displayOrder = (short)((maxDisplayOrder ?? 0) + 1);

            var skill = new Skill
            {
                SkillName = skillName,
                SkillCategory = skillCategory,
                SkillLevel = skillLevel,
                DisplayOrder = displayOrder
            };

            await _context.Skills.AddAsync(skill);

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation("Skill added successfully. SkillId: {SkillId}", skill.SkillId);

                return enOperationResult.Success;
            }

            _logger.LogWarning("Skill add operation failed.");

            return enOperationResult.Failed;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _logger.LogWarning("Cannot add skill because a unique constraint was violated. SkillName: {SkillName}, SkillCategory: {SkillCategory}", skillName, skillCategory);

            return enOperationResult.AlreadyExist;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding skill.");

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> UpdateSkillAsync(int skillId, string skillName, string skillCategory, byte skillLevel, short? displayOrder = null)
    {
        if (skillId <= 0 || string.IsNullOrWhiteSpace(skillName) || string.IsNullOrWhiteSpace(skillCategory))
            return enOperationResult.InvalidData;

        if (skillLevel is < 1 or > 5)
            return enOperationResult.InvalidSkillLevel;

        if (displayOrder.HasValue && displayOrder.Value < 1)
            return enOperationResult.InvalidData;

        skillName = skillName.Trim();
        skillCategory = skillCategory.Trim();

        try
        {
            var skill = await _context.Skills.FindAsync(skillId);

            if (skill == null)
            {
                _logger.LogWarning("Skill update failed. SkillId: {SkillId}", skillId);

                return enOperationResult.NotFound;
            }

            // Check if anything actually changed
            bool noChanges =
                skill.SkillName == skillName &&
                skill.SkillCategory == skillCategory &&
                skill.SkillLevel == skillLevel &&
                (!displayOrder.HasValue || skill.DisplayOrder == displayOrder.Value);

            if (noChanges)
            {
                _logger.LogInformation("Skill update skipped because no changes were detected. SkillId: {SkillId}", skillId);

                return enOperationResult.Success;
            }

            skill.SkillName = skillName;
            skill.SkillCategory = skillCategory;
            skill.SkillLevel = skillLevel;

            if (displayOrder.HasValue)
            {
                skill.DisplayOrder = displayOrder.Value;
            }

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation("Skill updated successfully. SkillId: {SkillId}", skillId);

                return enOperationResult.Success;
            }

            _logger.LogWarning("Skill update operation failed. SkillId: {SkillId}", skillId);

            return enOperationResult.Failed;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _logger.LogWarning("Cannot update skill because a unique constraint was violated. SkillId: {SkillId}, SkillName: {SkillName}", skillId, skillName);

            return enOperationResult.AlreadyExist;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547)
        {
            _logger.LogWarning("Cannot update skill because of a foreign key violation. SkillId: {SkillId}", skillId);

            return enOperationResult.InvalidData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating skill. SkillId: {SkillId}", skillId);

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> DeleteSkillAsync(int skillId)
    {
        if (skillId <= 0)
            return enOperationResult.InvalidData;

        try
        {
            var skill = await _context.Skills.FindAsync(skillId);

            if (skill == null)
            {
                _logger.LogWarning("Skill deletion failed. SkillId: {SkillId}", skillId);

                return enOperationResult.NotFound;
            }

            var projectSkills = await _context.ProjectSkills.Where(ps => ps.SkillId == skillId).ToListAsync();

            if (projectSkills.Count > 0)
                _context.ProjectSkills.RemoveRange(projectSkills);

            _context.Skills.Remove(skill);

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation("Skill deleted successfully. SkillId: {SkillId}", skill.SkillId);

                return enOperationResult.Success;
            }

            _logger.LogWarning("Skill deletion operation failed. SkillId: {SkillId}", skillId);

            return enOperationResult.Failed;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Skill deletion failed because the skill was modified or deleted concurrently. SkillId: {SkillId}", skillId);

            bool skillExists = await _context.Skills.AnyAsync(s => s.SkillId == skillId);

            return skillExists ? enOperationResult.Failed : enOperationResult.NotFound;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547)
        {
            _logger.LogWarning("Skill deletion was blocked because related records still reference it. SkillId: {SkillId}", skillId);

            return enOperationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting skill. SkillId: {SkillId}", skillId);

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> ReorderSkillsAsync(List<(int SkillId, short DisplayOrder)> items)
    {
        if (items == null || items.Count == 0)
            return enOperationResult.InvalidData;

        if (items.Any(x => x.SkillId <= 0 || x.DisplayOrder < 1))
            return enOperationResult.InvalidData;

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var ids = items.Select(x => x.SkillId).ToList();
            var skills = await _context.Skills
                .Where(s => ids.Contains(s.SkillId))
                .ToListAsync();

            if (skills.Count != ids.Count)
                return enOperationResult.NotFound;

            // Phase 1: Offset to safe temporary values
            foreach (var skill in skills)
            {
                skill.DisplayOrder += 10000;
            }
            await _context.SaveChangesAsync();

            // Phase 2: Set final values
            var dict = items.ToDictionary(x => x.SkillId, x => x.DisplayOrder);
            foreach (var skill in skills)
            {
                skill.DisplayOrder = dict[skill.SkillId];
            }
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogInformation("Skills reordered successfully. Count: {Count}", items.Count);

            return enOperationResult.Success;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _logger.LogWarning("Skill reorder failed due to a unique constraint violation.");

            return enOperationResult.AlreadyExist;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reordering skills.");

            return enOperationResult.Failed;
        }
    }
}