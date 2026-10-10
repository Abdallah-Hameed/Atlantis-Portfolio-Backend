using System;
using System.Collections.Generic;

namespace Atlantis_Portfolio_Backend_DAL.Entities;

public partial class AuditLog
{
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public int ActionId { get; set; }

    public bool Success { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Ipaddress { get; set; }

    public virtual Action Action { get; set; } = null!;

    public virtual User? User { get; set; }
}
