using Atlantis_Portfolio_Backend_BLL.Models;
using Atlantis_Portfolio_Backend_BLL.Services;
using Atlantis_Portfolio_Backend_DAL.Context;
using Atlantis_Portfolio_Backend_DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlantis_Portfolio_Backend_BLL;

public class clsAuthService
{
    private readonly AppDbContext _context;
    private readonly clsTokenService _tokenService;
    private readonly clsAuditLogService _auditLogService;
    private readonly ILogger<clsAuthService> _logger;

    public clsAuthService(AppDbContext context, clsTokenService tokenService, clsAuditLogService auditLogService, ILogger<clsAuthService> logger)
    {
        _context = context;

        _tokenService = tokenService;

        _auditLogService = auditLogService;

        _logger = logger;
    }

    public async Task<clsLoginResult> LoginAsync(string email, string password, string? ipAddress)
    {
        var loginResult = new clsLoginResult();

        if (string.IsNullOrWhiteSpace(email) ||string.IsNullOrWhiteSpace(password))
        {
            loginResult.Result = enOperationResult.InvalidData;

            _logger.LogWarning("User login failed due to invalid data.");

            return loginResult;
        }

        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                await _auditLogService.AddAsync(actionName: "FailedLogin", userId: null, success: false, ipAddress: ipAddress);

                loginResult.Result = enOperationResult.InvalidCredentials;

                _logger.LogWarning("User login failed due to invalid credentials.");

                return loginResult;
            }

            if (!user.IsActive)
            {
                loginResult.Result = enOperationResult.AccountInactive;

                _logger.LogWarning("User login failed because the account is inactive.");

                return loginResult;
            }

            if (user.LockoutUntil.HasValue && user.LockoutUntil.Value > DateTime.UtcNow) 
            {
                loginResult.Result = enOperationResult.AccountLocked;

                _logger.LogWarning("User login failed because the account is locked.");

                return loginResult;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) 
            {
                user.FailedLoginAttempts++;

                bool accountLocked = user.FailedLoginAttempts >= 3;

                if (accountLocked)
                {
                    user.LockoutUntil = DateTime.UtcNow.AddHours(1);
                }

                await _context.SaveChangesAsync();

                await _auditLogService.AddAsync(actionName: "FailedLogin", userId: user.UserId, success: false, ipAddress: ipAddress);

                if (accountLocked)
                {
                    await _auditLogService.AddAsync(actionName: "AccountLocked", userId: user.UserId, success: false, ipAddress: ipAddress);
                }

                loginResult.Result = enOperationResult.InvalidCredentials;

                _logger.LogWarning(
                    "User login failed due to invalid credentials.");

                return loginResult;
            }

            user.FailedLoginAttempts = 0;
            user.LockoutUntil = null;
            user.LastLoginAt = DateTime.UtcNow;

            string accessToken =
                _tokenService.GenerateAccessToken(user);

            string refreshToken =
                _tokenService.GenerateRefreshToken();

            byte[] refreshTokenHash =
                _tokenService.HashRefreshToken(refreshToken);

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = Convert.ToHexString(refreshTokenHash),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            await _context.RefreshTokens.AddAsync(refreshTokenEntity);
            await _context.SaveChangesAsync();

            await _auditLogService.AddAsync(actionName: "Login", userId: user.UserId, success: true, ipAddress: ipAddress);

            loginResult.Result = enOperationResult.Success;
            loginResult.User = user;
            loginResult.AccessToken = accessToken;
            loginResult.RefreshToken = refreshToken;

            _logger.LogInformation(
                "User login successful for user ID: {UserId}.",
                user.UserId);

            return loginResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred during user login.");

            loginResult.Result = enOperationResult.Failed;

            return loginResult;
        }
    }

    public async Task<clsLoginResult> RefreshAsync(string refreshToken,string? ipAddress)
    {
        var loginResult = new clsLoginResult();

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            loginResult.Result = enOperationResult.InvalidData;

            _logger.LogWarning(
                "Refresh token failed due to invalid data.");

            return loginResult;
        }

        try
        {
            string tokenHash = Convert.ToHexString(
                _tokenService.HashRefreshToken(refreshToken));

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var storedRefreshToken = await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.TokenHash == tokenHash);

            if (storedRefreshToken == null)
            {
                loginResult.Result = enOperationResult.InvalidCredentials;

                _logger.LogWarning(
                    "Refresh token failed because the token was not found.");

                return loginResult;
            }

            if (storedRefreshToken.RevokedAt.HasValue)
            {
                loginResult.Result = enOperationResult.InvalidCredentials;

                _logger.LogWarning(
                    "Refresh token failed because the token was revoked.");

                return loginResult;
            }

            if (storedRefreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                loginResult.Result = enOperationResult.InvalidCredentials;

                _logger.LogWarning(
                    "Refresh token failed because the token has expired.");

                return loginResult;
            }

            if (!storedRefreshToken.User.IsActive)
            {
                loginResult.Result = enOperationResult.AccountInactive;

                _logger.LogWarning(
                    "Refresh token failed because the user account is inactive.");

                return loginResult;
            }

            DateTime now = DateTime.UtcNow;

            int affectedRows = await _context.RefreshTokens
                .Where(r =>
                    r.RefreshTokenId ==
                        storedRefreshToken.RefreshTokenId &&
                    r.RevokedAt == null &&
                    r.ExpiresAt > now)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(r => r.RevokedAt, now));

            if (affectedRows != 1)
            {
                await transaction.RollbackAsync();

                loginResult.Result =
                    enOperationResult.InvalidCredentials;

                _logger.LogWarning(
                    "Refresh token failed because it was already consumed.");

                return loginResult;
            }

            string newAccessToken =
                _tokenService.GenerateAccessToken(
                    storedRefreshToken.User);

            string newRefreshToken =
                _tokenService.GenerateRefreshToken();

            string newRefreshTokenHash = Convert.ToHexString(
                _tokenService.HashRefreshToken(newRefreshToken));

            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = storedRefreshToken.UserId,
                TokenHash = newRefreshTokenHash,
                ExpiresAt = now.AddDays(7),
                CreatedAt = now
            };

            await _context.RefreshTokens.AddAsync(
                newRefreshTokenEntity);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _auditLogService.AddAsync(
                actionName: "RefreshToken",
                userId: storedRefreshToken.UserId,
                success: true,
                ipAddress: ipAddress);

            loginResult.Result = enOperationResult.Success;
            loginResult.User = storedRefreshToken.User;
            loginResult.AccessToken = newAccessToken;
            loginResult.RefreshToken = newRefreshToken;

            _logger.LogInformation(
                "Refresh token successfully renewed for user ID: {UserId}.",
                storedRefreshToken.UserId);

            return loginResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while refreshing access token.");

            loginResult.Result = enOperationResult.Failed;

            return loginResult;
        }
    }

    public async Task<enOperationResult> LogoutAsync(
        string refreshToken,string? ipAddrss)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            _logger.LogWarning(
                "User logout failed due to invalid data.");

            return enOperationResult.InvalidData;
        }

        try
        {
            byte[] refreshTokenHash =
                _tokenService.HashRefreshToken(refreshToken);

            string tokenHash =
                Convert.ToHexString(refreshTokenHash);

            var storedRefreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.TokenHash == tokenHash);

            if (storedRefreshToken == null)
            {
                _logger.LogWarning(
                    "User logout failed because the refresh token was not found.");

                return enOperationResult.InvalidCredentials;
            }

            if (storedRefreshToken.RevokedAt.HasValue)
            {
                _logger.LogWarning(
                    "User logout failed because the refresh token was already revoked.");

                return enOperationResult.InvalidCredentials;
            }

            storedRefreshToken.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditLogService.AddAsync(
                actionName: "Logout",
                userId: storedRefreshToken.UserId,
                success: true,
                ipAddress: ipAddrss);

            _logger.LogInformation(
                "User logout successful for user ID: {UserId}.",
                storedRefreshToken.UserId);

            return enOperationResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while logging out user.");

            return enOperationResult.Failed;
        }
    }
}