using System;

namespace Atlantis_Portfolio_Backend.DTO;

public class clsAuditLogDto
{
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public int ActionId { get; set; }

    public string ActionName { get; set; } = null!;

    public bool Success { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Ipaddress { get; set; }

}