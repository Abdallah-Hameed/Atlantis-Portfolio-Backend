using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Atlantis_Portfolio_Backend_BLL;
using Atlantis_Portfolio_Backend_BLL.Models;
using Atlantis_Portfolio_Backend_DAL.Context;
using Atlantis_Portfolio_Backend_DAL.Entities;

namespace Atlantis_Portfolio_Backend_Tests;

public class clsAuthServiceTests
{
    private (AppDbContext Context, SqliteConnection Connection) GetSqliteDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");

        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;

        var context = new AppDbContext(options);

        context.Database.EnsureCreated();

        return (context, connection);
    }

    [Fact]
    public async Task LoginAsync_Success_ReturnsSuccessAndTokens()
    {
        // Arrange
        var (context, connection) = GetSqliteDbContext();

        using var _ = context;

        using var __ = connection;

        var tokenServiceMock = new Mock<clsTokenService>(Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>());

        var auditLogServiceMock = new Mock<clsAuditLogService>(context, Mock.Of<ILogger<clsAuditLogService>>());

        var loggerMock = new Mock<ILogger<clsAuthService>>();

        var password = "Password123!";

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = hashedPassword,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        tokenServiceMock.Setup(s => s.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");

        tokenServiceMock.Setup(s => s.GenerateRefreshToken()).Returns("refresh-token");

        tokenServiceMock.Setup(s => s.HashRefreshToken("refresh-token")).Returns(Encoding.UTF8.GetBytes("refresh-token-hash"));

        auditLogServiceMock.Setup(s => s.AddAsync("Login", user.UserId, true, "127.0.0.1")).ReturnsAsync(enOperationResult.Success);

        var authService = new clsAuthService(context, tokenServiceMock.Object, auditLogServiceMock.Object, loggerMock.Object);

        // Act
        var result = await authService.LoginAsync("test@example.com", password, "127.0.0.1");

        // Assert
        result.Should().NotBeNull();

        result.Result.Should().Be(enOperationResult.Success);

        result.AccessToken.Should().Be("access-token");

        result.RefreshToken.Should().Be("refresh-token");

        result.User.Should().NotBeNull();

        result.User!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_UserNotFound_ReturnsInvalidCredentials()
    {
        // Arrange
        var (context, connection) = GetSqliteDbContext();

        using var _ = context;

        using var __ = connection;

        var tokenServiceMock = new Mock<clsTokenService>(Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>());

        var auditLogServiceMock = new Mock<clsAuditLogService>(context, Mock.Of<ILogger<clsAuditLogService>>());

        var loggerMock = new Mock<ILogger<clsAuthService>>();

        auditLogServiceMock.Setup(s => s.AddAsync("FailedLogin", null, false, "127.0.0.1")).ReturnsAsync(enOperationResult.Success);

        var authService = new clsAuthService(context, tokenServiceMock.Object, auditLogServiceMock.Object, loggerMock.Object);

        // Act
        var result = await authService.LoginAsync("nonexistent@example.com", "Password123!", "127.0.0.1");

        // Assert
        result.Should().NotBeNull();

        result.Result.Should().Be(enOperationResult.InvalidCredentials);
    }
    [Fact]
    public async Task LoginAsync_InvalidCredentials_WrongPassword_ReturnsInvalidCredentials()
    {
        // Arrange
        var (context, connection) = GetSqliteDbContext();

        using var _ = context;

        using var __ = connection;

        var tokenServiceMock = new Mock<clsTokenService>(Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>());

        var auditLogServiceMock = new Mock<clsAuditLogService>(context, Mock.Of<ILogger<clsAuditLogService>>());

        var loggerMock = new Mock<ILogger<clsAuthService>>();

        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        auditLogServiceMock.Setup(s => s.AddAsync("FailedLogin", user.UserId, false, "127.0.0.1")).ReturnsAsync(enOperationResult.Success);

        var authService = new clsAuthService(context, tokenServiceMock.Object, auditLogServiceMock.Object, loggerMock.Object);

        // Act
        var result = await authService.LoginAsync("test@example.com", "WrongPassword", "127.0.0.1");

        // Assert
        result.Should().NotBeNull();

        result.Result.Should().Be(enOperationResult.InvalidCredentials);
    }

    [Fact]
    public async Task LoginAsync_AccountLocked_ReturnsAccountLocked()
    {
        // Arrange
        var (context, connection) = GetSqliteDbContext();

        using var _ = context;

        using var __ = connection;

        var tokenServiceMock = new Mock<clsTokenService>(Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>());

        var auditLogServiceMock = new Mock<clsAuditLogService>(context, Mock.Of<ILogger<clsAuditLogService>>());

        var loggerMock = new Mock<ILogger<clsAuthService>>();

        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            IsActive = true,
            LockoutUntil = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        var authService = new clsAuthService(context, tokenServiceMock.Object, auditLogServiceMock.Object, loggerMock.Object);

        // Act
        var result = await authService.LoginAsync("test@example.com", "Password123!", "127.0.0.1");

        // Assert
        result.Should().NotBeNull();

        result.Result.Should().Be(enOperationResult.AccountLocked);
    }

    [Fact]
    public async Task LoginAsync_InactiveAccount_ReturnsAccountInactive()
    {
        // Arrange
        var (context, connection) = GetSqliteDbContext();

        using var _ = context;

        using var __ = connection;

        var tokenServiceMock = new Mock<clsTokenService>(Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>());

        var auditLogServiceMock = new Mock<clsAuditLogService>(context, Mock.Of<ILogger<clsAuditLogService>>());

        var loggerMock = new Mock<ILogger<clsAuthService>>();

        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        var authService = new clsAuthService(context, tokenServiceMock.Object, auditLogServiceMock.Object, loggerMock.Object);

        // Act
        var result = await authService.LoginAsync("test@example.com", "Password123!", "127.0.0.1");

        // Assert
        result.Should().NotBeNull();

        result.Result.Should().Be(enOperationResult.AccountInactive);
    }

    [Fact]
    public async Task RefreshAsync_Success_ReturnsSuccessAndNewTokens()
    {
        // Arrange
        var (context, connection) = GetSqliteDbContext();

        using var _ = context;

        using var __ = connection;

        var tokenServiceMock = new Mock<clsTokenService>(Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>());

        var auditLogServiceMock = new Mock<clsAuditLogService>(context, Mock.Of<ILogger<clsAuditLogService>>());

        var loggerMock = new Mock<ILogger<clsAuthService>>();

        var user = new User
        {
            UserId = 1,
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        string refreshTokenStr = "valid-refresh-token";

        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshTokenStr));

        string tokenHashHex = Convert.ToHexString(hashBytes);

        var refreshTokenEntity = new RefreshToken
        {
            RefreshTokenId = 1,
            UserId = 1,
            User = user,
            TokenHash = tokenHashHex,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);

        context.RefreshTokens.Add(refreshTokenEntity);

        await context.SaveChangesAsync();

        tokenServiceMock.Setup(s => s.HashRefreshToken(refreshTokenStr)).Returns(hashBytes);

        tokenServiceMock.Setup(s => s.GenerateAccessToken(user)).Returns("new-access-token");

        tokenServiceMock.Setup(s => s.GenerateRefreshToken()).Returns("new-refresh-token");

        tokenServiceMock.Setup(s => s.HashRefreshToken("new-refresh-token")).Returns(new byte[32]);

        auditLogServiceMock.Setup(s => s.AddAsync("RefreshToken", 1, true, "127.0.0.1")).ReturnsAsync(enOperationResult.Success);

        var authService = new clsAuthService(context, tokenServiceMock.Object, auditLogServiceMock.Object, loggerMock.Object);

        // Act
        var result = await authService.RefreshAsync(refreshTokenStr, "127.0.0.1");

        // Assert
        result.Should().NotBeNull();

        result.Result.Should().Be(enOperationResult.Success);

        result.AccessToken.Should().Be("new-access-token");

        result.RefreshToken.Should().Be("new-refresh-token");
    }

    [Fact]
    public async Task RefreshAsync_InvalidOrExpiredRefreshToken_ReturnsInvalidCredentials()
    {
        // Arrange
        var (context, connection) = GetSqliteDbContext();

        using var _ = context;

        using var __ = connection;

        var tokenServiceMock = new Mock<clsTokenService>(Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>());

        var auditLogServiceMock = new Mock<clsAuditLogService>(context, Mock.Of<ILogger<clsAuditLogService>>());

        var loggerMock = new Mock<ILogger<clsAuthService>>();

        tokenServiceMock.Setup(s => s.HashRefreshToken(It.IsAny<string>())).Returns(new byte[32]);

        var authService = new clsAuthService(context, tokenServiceMock.Object, auditLogServiceMock.Object, loggerMock.Object);

        // Act
        var result = await authService.RefreshAsync("nonexistent-token", "127.0.0.1");

        // Assert
        result.Should().NotBeNull();

        result.Result.Should().Be(enOperationResult.InvalidCredentials);
    }

    [Fact]
    public async Task RefreshAsync_InactiveAccount_ReturnsAccountInactive()
    {
        // Arrange
        var (context, connection) = GetSqliteDbContext();

        using var _ = context;

        using var __ = connection;

        var tokenServiceMock = new Mock<clsTokenService>(Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>());

        var auditLogServiceMock = new Mock<clsAuditLogService>(context, Mock.Of<ILogger<clsAuditLogService>>());

        var loggerMock = new Mock<ILogger<clsAuthService>>();

        var user = new User
        {
            UserId = 1,
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        string refreshTokenStr = "valid-refresh-token";

        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshTokenStr));

        string tokenHashHex = Convert.ToHexString(hashBytes);

        var refreshTokenEntity = new RefreshToken
        {
            RefreshTokenId = 1,
            UserId = 1,
            User = user,
            TokenHash = tokenHashHex,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);

        context.RefreshTokens.Add(refreshTokenEntity);

        await context.SaveChangesAsync();

        tokenServiceMock.Setup(s => s.HashRefreshToken(refreshTokenStr)).Returns(hashBytes);

        var authService = new clsAuthService(context, tokenServiceMock.Object, auditLogServiceMock.Object, loggerMock.Object);

        // Act
        var result = await authService.RefreshAsync(refreshTokenStr, "127.0.0.1");

        // Assert
        result.Should().NotBeNull();

        result.Result.Should().Be(enOperationResult.AccountInactive);
    }

}