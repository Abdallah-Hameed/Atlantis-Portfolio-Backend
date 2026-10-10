using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Atlantis_Portfolio_Backend_BLL;
using Atlantis_Portfolio_Backend_BLL.Services;
using Atlantis_Portfolio_Backend_BLL.Models;
using Atlantis_Portfolio_Backend_DAL.Context;
using Atlantis_Portfolio_Backend_DAL.Entities;

namespace Atlantis_Portfolio_Backend_Tests;

public class clsUserServiceTests
{
    private const string MasterPassword = "Master@123456";

    private (AppDbContext Context, SqliteConnection Connection) GetSqliteDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();

        return (context, connection);
    }

    private static SecuritySetting CreateSecuritySetting(string plainMasterPassword)
    {
        return new SecuritySetting
        {
            CriticalActionSecretHash = BCrypt.Net.BCrypt.HashPassword(plainMasterPassword),
            FailedAttempts = 0,
            LockoutUntil = null
        };
    }

    private static User CreateUser(string email = "user@example.com")
    {
        return new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
            IsActive = true,
            FailedLoginAttempts = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ============================================================
    // TEST 1: Success path
    // ============================================================
    [Fact]
    public async Task DeleteUserAsync_WithValidMasterPassword_DeletesUserAndReturnsSuccess()
    {
        // Arrange
        var (context, connection) = GetSqliteDbContext();
        using var _ = context;
        using var __ = connection;

        var user = CreateUser();
        context.Users.Add(user);
        context.SecuritySettings.Add(CreateSecuritySetting(MasterPassword));
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<clsUserService>>();
        var userService = new clsUserService(context, loggerMock.Object);

        // Act
        var result = await userService.DeleteUserAsync(user.UserId, MasterPassword, "127.0.0.1");

        // Assert
        result.Should().Be(enOperationResult.Success);

        var userExists = await context.Users.AnyAsync(u => u.UserId == user.UserId);
        userExists.Should().BeFalse("the user should be removed from the database");
    }

    // ============================================================
    // TEST 2: Invalid master password → 401 + counter increments
    // ============================================================
    [Fact]
    public async Task DeleteUserAsync_WithInvalidMasterPassword_ReturnsInvalidCredentialsAndIncrementsFailedAttempts()
    {
        // Arrange
        var (context, connection) = GetSqliteDbContext();
        using var _ = context;
        using var __ = connection;

        var user = CreateUser();
        context.Users.Add(user);
        context.SecuritySettings.Add(CreateSecuritySetting(MasterPassword));
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<clsUserService>>();
        var userService = new clsUserService(context, loggerMock.Object);

        // Act
        var result = await userService.DeleteUserAsync(user.UserId, "WrongMaster@999", "127.0.0.1");

        // Assert
        result.Should().Be(enOperationResult.InvalidCredentials);

        var security = await context.SecuritySettings.FirstAsync();
        security.FailedAttempts.Should().Be(1);
        security.LockoutUntil.Should().BeNull();

        var userExists = await context.Users.AnyAsync(u => u.UserId == user.UserId);
        userExists.Should().BeTrue("the user should NOT be deleted when the password is wrong");
    }

    // ============================================================
    // TEST 3: Third failed attempt triggers lockout
    // ============================================================
    [Fact]
    public async Task DeleteUserAsync_AfterThreeFailedAttempts_LocksOut()
    {
        // Arrange
        var (context, connection) = GetSqliteDbContext();
        using var _ = context;
        using var __ = connection;

        var user = CreateUser();
        var security = CreateSecuritySetting(MasterPassword);
        security.FailedAttempts = 2; // already failed twice
        context.Users.Add(user);
        context.SecuritySettings.Add(security);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<clsUserService>>();
        var userService = new clsUserService(context, loggerMock.Object);

        // Act
        var result = await userService.DeleteUserAsync(user.UserId, "WrongMaster@999", "127.0.0.1");

        // Assert
        result.Should().Be(enOperationResult.InvalidCredentials);

        var updated = await context.SecuritySettings.FirstAsync();
        updated.FailedAttempts.Should().Be(3);
        updated.LockoutUntil.Should().NotBeNull();
        updated.LockoutUntil!.Value.Should()
            .BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromMinutes(1));
    }

    // ============================================================
    // TEST 4: Account is locked → reject even with correct password
    // ============================================================
    [Fact]
    public async Task DeleteUserAsync_WhenLockedOut_ReturnsAccountLocked()
    {
        // Arrange
        var (context, connection) = GetSqliteDbContext();
        using var _ = context;
        using var __ = connection;

        var user = CreateUser();
        var security = CreateSecuritySetting(MasterPassword);
        security.FailedAttempts = 3;
        security.LockoutUntil = DateTime.UtcNow.AddHours(1); // currently locked
        context.Users.Add(user);
        context.SecuritySettings.Add(security);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<clsUserService>>();
        var userService = new clsUserService(context, loggerMock.Object);

        // Act — even with the correct master password, lockout must block
        var result = await userService.DeleteUserAsync(user.UserId, MasterPassword, "127.0.0.1");

        // Assert
        result.Should().Be(enOperationResult.AccountLocked);

        var userExists = await context.Users.AnyAsync(u => u.UserId == user.UserId);
        userExists.Should().BeTrue("the user should NOT be deleted during lockout");
    }

    // ============================================================
    // TEST 5: Target user does not exist
    // ============================================================
    [Fact]
    public async Task DeleteUserAsync_WithNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var (context, connection) = GetSqliteDbContext();
        using var _ = context;
        using var __ = connection;

        context.SecuritySettings.Add(CreateSecuritySetting(MasterPassword));
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<clsUserService>>();
        var userService = new clsUserService(context, loggerMock.Object);

        // Act
        var result = await userService.DeleteUserAsync(99999, MasterPassword, "127.0.0.1");

        // Assert
        result.Should().Be(enOperationResult.NotFound);
    }

    // ============================================================
    // TEST 6: SecuritySetting row is missing → fail safely
    // ============================================================
    [Fact]
    public async Task DeleteUserAsync_WhenSecuritySettingMissing_ReturnsFailed()
    {
        // Arrange
        var (context, connection) = GetSqliteDbContext();
        using var _ = context;
        using var __ = connection;

        var user = CreateUser();
        context.Users.Add(user);
        // NOTE: intentionally NOT adding a SecuritySetting
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<clsUserService>>();
        var userService = new clsUserService(context, loggerMock.Object);

        // Act
        var result = await userService.DeleteUserAsync(user.UserId, MasterPassword, "127.0.0.1");

        // Assert
        result.Should().Be(enOperationResult.Failed);

        var userExists = await context.Users.AnyAsync(u => u.UserId == user.UserId);
        userExists.Should().BeTrue("the user should NOT be deleted when security settings are missing");
    }

    // ============================================================
    // TEST 7: Successful delete resets the failed counter
    // ============================================================
    [Fact]
    public async Task DeleteUserAsync_AfterSuccessfulDelete_ResetsFailedAttempts()
    {
        // Arrange
        var (context, connection) = GetSqliteDbContext();
        using var _ = context;
        using var __ = connection;

        var user = CreateUser();
        var security = CreateSecuritySetting(MasterPassword);
        security.FailedAttempts = 2; // previous failed attempts
        context.Users.Add(user);
        context.SecuritySettings.Add(security);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<clsUserService>>();
        var userService = new clsUserService(context, loggerMock.Object);

        // Act
        var result = await userService.DeleteUserAsync(user.UserId, MasterPassword, "127.0.0.1");

        // Assert
        result.Should().Be(enOperationResult.Success);

        var updated = await context.SecuritySettings.FirstAsync();
        updated.FailedAttempts.Should().Be(0, "the counter should be reset after a successful critical action");
        updated.LockoutUntil.Should().BeNull();
    }

    // ============================================================
    // TEST 8: Invalid input (targetUserId <= 0 or empty password)
    // ============================================================
    [Fact]
    public async Task DeleteUserAsync_WithInvalidData_ReturnsInvalidData()
    {
        // Arrange
        var (context, connection) = GetSqliteDbContext();
        using var _ = context;
        using var __ = connection;

        var loggerMock = new Mock<ILogger<clsUserService>>();
        var userService = new clsUserService(context, loggerMock.Object);

        // Act
        var resultZeroId = await userService.DeleteUserAsync(0, MasterPassword, "127.0.0.1");
        var resultEmptyPassword = await userService.DeleteUserAsync(1, "", "127.0.0.1");

        // Assert
        resultZeroId.Should().Be(enOperationResult.InvalidData);
        resultEmptyPassword.Should().Be(enOperationResult.InvalidData);
    }
}