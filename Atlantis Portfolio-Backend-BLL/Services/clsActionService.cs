using Atlantis_Portfolio_Backend_DAL;
using Atlantis_Portfolio_Backend_DAL.Context;
using Atlantis_Portfolio_Backend_DAL.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ActionEntity = Atlantis_Portfolio_Backend_DAL.Entities.Action;

namespace Atlantis_Portfolio_Backend_BLL.Services;

public class clsActionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<clsActionService> _logger;

    public clsActionService(
        AppDbContext context,
        ILogger<clsActionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ActionEntity>> GetAllAsync()
    {
        return await _context.Set<ActionEntity>()
            .AsNoTracking()
            .OrderBy(a => a.ActionId)
            .ToListAsync();
    }

    public async Task<ActionEntity?> GetByIdAsync(int actionId)
    {
        return await _context.Set<ActionEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ActionId == actionId);
    }

    public async Task<enOperationResult> AddAsync(string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
            return enOperationResult.InvalidData;

        actionName = actionName.Trim();

        if (actionName.Length > 50)
            return enOperationResult.InvalidData;

        try
        {
            bool exists = await _context.Set<ActionEntity>()
                .AnyAsync(a => a.ActionName == actionName);

            if (exists)
                return enOperationResult.Conflict;

            var action = new ActionEntity
            {
                ActionName = actionName
            };

            _context.Set<ActionEntity>().Add(action);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Action {ActionId} added successfully.",
                action.ActionId);

            return enOperationResult.Success;
        }
        catch (DbUpdateException ex)
            when (IsUniqueConstraintViolation(ex))
        {
            return enOperationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add Action.");

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> UpdateAsync(
        int actionId,
        string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
            return enOperationResult.InvalidData;

        actionName = actionName.Trim();

        if (actionName.Length > 50)
            return enOperationResult.InvalidData;

        try
        {
            var action = await _context.Set<ActionEntity>()
                .FirstOrDefaultAsync(a => a.ActionId == actionId);

            if (action == null)
                return enOperationResult.NotFound;

            bool duplicate = await _context.Set<ActionEntity>()
                .AnyAsync(a =>
                    a.ActionId != actionId &&
                    a.ActionName == actionName);

            if (duplicate)
                return enOperationResult.Conflict;

            action.ActionName = actionName;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Action {ActionId} updated successfully.",
                actionId);

            return enOperationResult.Success;
        }
        catch (DbUpdateException ex)
            when (IsUniqueConstraintViolation(ex))
        {
            return enOperationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to update Action {ActionId}.",
                actionId);

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> DeleteAsync(int actionId)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var action = await _context.Set<ActionEntity>()
                .FirstOrDefaultAsync(a => a.ActionId == actionId);

            if (action == null)
                return enOperationResult.NotFound;

            // Delete related audit logs first.
            await _context.Set<AuditLog>()
                .Where(log => log.ActionId == actionId)
                .ExecuteDeleteAsync();

            _context.Set<ActionEntity>().Remove(action);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Action {ActionId} and its related audit logs deleted.",
                actionId);

            return enOperationResult.Success;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            _logger.LogError(
                ex,
                "Failed to delete Action {ActionId}.",
                actionId);

            return enOperationResult.Failed;
        }
    }

    private static bool IsUniqueConstraintViolation(
        DbUpdateException ex)
    {
        return ex.InnerException is SqlException sqlException
            && (sqlException.Number == 2601
                || sqlException.Number == 2627);
    }
}