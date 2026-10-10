using Atlantis_Portfolio_Backend_BLL.Models;
using Atlantis_Portfolio_Backend_DAL.Context;
using Atlantis_Portfolio_Backend_DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ActionEntity = Atlantis_Portfolio_Backend_DAL.Entities.Action;

namespace Atlantis_Portfolio_Backend_BLL
{
    public class clsAuditLogService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<clsAuditLogService> _logger;

        public clsAuditLogService(
            AppDbContext context,
            ILogger<clsAuditLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public virtual async Task<enOperationResult> AddAsync(string actionName, int? userId, bool success, string? ipAddress)
        {
            if (string.IsNullOrWhiteSpace(actionName))
                return enOperationResult.InvalidData;

            if (ipAddress?.Length > 45)
                return enOperationResult.InvalidData;

            try
            {
                int? actionId = await _context.Set<ActionEntity>().AsNoTracking().Where(a => a.ActionName == actionName).Select(a => (int?)a.ActionId).FirstOrDefaultAsync();

                if (actionId == null)
                {
                    _logger.LogWarning(
                        "Audit log action {ActionName} was not found.",
                        actionName);

                    return enOperationResult.NotFound;
                }

                var auditLog = new AuditLog
                {
                    ActionId = actionId.Value,
                    UserId = userId,
                    Success = success,
                    CreatedAt = DateTime.UtcNow,
                    Ipaddress = ipAddress
                };

                _context.Set<AuditLog>().Add(auditLog);

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Audit log created for action {ActionName}.",
                    actionName);

                return enOperationResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create audit log for action {ActionName}.",
                    actionName);

                return enOperationResult.Failed;
            }
        }

        public async Task<PaginatedResult<AuditLog>> GetAllAuditLogsAsync(int pageNumber = 1, int pageSize = 20)
        {
            if (pageSize > 100) pageSize = 100;
            if (pageNumber < 1) pageNumber = 1;

            try
            {
                var query = _context.Set<AuditLog>()
                    .AsNoTracking()
                    .Include(log => log.Action)
                    .Include(log => log.User);

                int totalCount = await query.CountAsync();
                int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var items = await query
                    .OrderByDescending(log => log.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PaginatedResult<AuditLog>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving audit logs.");
                throw;
            }
        }
    }
}