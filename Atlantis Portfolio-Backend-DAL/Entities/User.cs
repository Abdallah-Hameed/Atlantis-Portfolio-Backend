using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Atlantis_Portfolio_Backend_DAL.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool IsActive { get; set; }

    public byte FailedLoginAttempts { get; set; }

    public DateTime? LockoutUntil { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    [JsonIgnore]
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}