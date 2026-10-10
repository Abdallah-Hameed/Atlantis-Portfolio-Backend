using Atlantis_Portfolio_Backend_DAL.Context;
using Atlantis_Portfolio_Backend_DAL.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlantis_Portfolio_Backend_BLL;

public class clsUserService
{
    private readonly AppDbContext _context;
    private readonly ILogger<clsUserService> _logger;

    public clsUserService(AppDbContext context, ILogger<clsUserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        if (userId <= 0)
            return null;

        try
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user. UserId: {UserId}", userId);

            throw;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        email = email.Trim();

        try
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user by email.");

            throw;
        }
    }

    public async Task<enOperationResult> AddUserAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return enOperationResult.InvalidData;

        email = email.Trim();

        try
        {
            var emailExists = await _context.Users.AnyAsync(u => u.Email == email);

            if (emailExists)
            {
                _logger.LogWarning("User creation failed because the email already exists.");

                return enOperationResult.AlreadyExist;
            }

            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsActive = true,
                FailedLoginAttempts = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation("User created successfully. UserId: {UserId}", user.UserId);

                return enOperationResult.Success;
            }

            _logger.LogWarning("User creation operation failed.");

            return enOperationResult.Failed;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _logger.LogWarning("User creation failed because the email already exists.");

            return enOperationResult.AlreadyExist;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating user.");

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> SetUserActiveAsync(int userId, bool isActive)
    {
        if (userId <= 0)
            return enOperationResult.InvalidData;

        try
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User status update failed. UserId: {UserId}", userId);

                return enOperationResult.NotFound;
            }

            if (user.IsActive == isActive)
                return enOperationResult.Success;

            user.IsActive = isActive;

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation("User status updated successfully. UserId: {UserId}", userId);

                return enOperationResult.Success;
            }

            _logger.LogWarning("User status update operation failed. UserId: {UserId}", userId);

            return enOperationResult.Failed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user status. UserId: {UserId}", userId);

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> ChangePasswordAsync(
        int userId,
        string currentPassword,
        string newPassword)
    {
        if (userId <= 0 ||
            string.IsNullOrWhiteSpace(currentPassword) ||
            string.IsNullOrWhiteSpace(newPassword))
        {
            return enOperationResult.InvalidData;
        }

        try
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                _logger.LogWarning(
                    "Password change failed. UserId: {UserId}", userId);

                return enOperationResult.NotFound;
            }

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                _logger.LogWarning(
                    "Password change failed due to invalid credentials. UserId: {UserId}",
                    userId);

                return enOperationResult.InvalidCredentials;
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            DateTime now = DateTime.UtcNow;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            await _context.SaveChangesAsync();

            int revokedTokensCount = await _context.RefreshTokens
                .Where(r =>
                    r.UserId == userId &&
                    r.RevokedAt == null)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(r => r.RevokedAt, now));

            await transaction.CommitAsync();

            _logger.LogInformation(
                "Password changed successfully and {RevokedTokensCount} refresh tokens revoked. UserId: {UserId}",
                revokedTokensCount,
                userId);

            return enOperationResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while changing password. UserId: {UserId}",
                userId);

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> ChangeEmailAsync(int userId, string email)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(email))
            return enOperationResult.InvalidData;

        email = email.Trim();

        try
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("Email change failed. UserId: {UserId}", userId);

                return enOperationResult.NotFound;
            }

            if (user.Email == email)
                return enOperationResult.Success;

            var emailExists = await _context.Users.AnyAsync(u => u.Email == email && u.UserId != userId);

            if (emailExists)
            {
                _logger.LogWarning("Email change failed because the email already exists. UserId: {UserId}", userId);

                return enOperationResult.AlreadyExist;
            }

            user.Email = email;

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation("Email changed successfully. UserId: {UserId}", userId);

                return enOperationResult.Success;
            }

            _logger.LogWarning("Email change operation failed. UserId: {UserId}", userId);

            return enOperationResult.Failed;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _logger.LogWarning("Email change failed because the email already exists. UserId: {UserId}", userId);

            return enOperationResult.AlreadyExist;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while changing email. UserId: {UserId}", userId);

            return enOperationResult.Failed;
        }
    }

    public async Task<enOperationResult> DeleteUserAsync(int targetUserId, string masterPassword, string? ipAddress)
    {
        if (targetUserId <= 0 || string.IsNullOrWhiteSpace(masterPassword))
            return enOperationResult.InvalidData;

        try
        {

            var security = await _context.SecuritySettings.FirstOrDefaultAsync();

            if (security == null || string.IsNullOrWhiteSpace(security.CriticalActionSecretHash))
            {
                _logger.LogError("SecuritySetting row is missing or master password hash is not configured.");

                return enOperationResult.Failed;
            }


            if (security.LockoutUntil.HasValue && security.LockoutUntil.Value > DateTime.UtcNow)
            {
                _logger.LogWarning("Critical action rejected: locked out until {LockoutUntil}.", security.LockoutUntil);

                return enOperationResult.AccountLocked;
            }


            if (!BCrypt.Net.BCrypt.Verify(masterPassword, security.CriticalActionSecretHash))
            {
                security.FailedAttempts++;

                if (security.FailedAttempts >= 3)
                {
                    security.LockoutUntil = DateTime.UtcNow.AddHours(1);

                    _logger.LogWarning("Critical action locked after {Attempts} failed attempts.", security.FailedAttempts);
                }

                await _context.SaveChangesAsync();

                _logger.LogWarning("Critical action failed: invalid master password. Attempts: {Attempts}.", security.FailedAttempts);

                return enOperationResult.InvalidCredentials;
            }

            var user = await _context.Users.FindAsync(targetUserId);

            if (user == null)
            {
                _logger.LogWarning("User delete failed: user {UserId} not found.", targetUserId);

                return enOperationResult.NotFound;
            }

            _context.Users.Remove(user);

  
            security.FailedAttempts = 0;

            security.LockoutUntil = null;

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation("User {UserId} deleted successfully.", targetUserId);

                return enOperationResult.Success;
            }

            _logger.LogWarning("User delete failed: no rows affected. UserId: {UserId}.", targetUserId);

            return enOperationResult.Failed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting user {UserId}.", targetUserId);

            return enOperationResult.Failed;
        }
    }
}