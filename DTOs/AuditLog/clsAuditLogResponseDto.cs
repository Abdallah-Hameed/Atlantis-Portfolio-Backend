namespace Atlantis_Portfolio_Backend.DTOs.AuditLogs;

public class clsAuditLogResponseDto
{
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public int ActionId { get; set; }

    public bool Success { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Ipaddress { get; set; }

    public string? ActionName { get; set; }

    public string? UserEmail { get; set; }
}