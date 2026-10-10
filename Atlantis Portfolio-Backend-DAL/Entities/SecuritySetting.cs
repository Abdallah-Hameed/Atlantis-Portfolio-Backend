using System;
using System.Collections.Generic;

namespace Atlantis_Portfolio_Backend_DAL.Entities;

public partial class SecuritySetting
{
    public int SecuritySettingId { get; set; }

    public string CriticalActionSecretHash { get; set; } = null!;

    public byte FailedAttempts { get; set; }

    public DateTime? LockoutUntil { get; set; }
}
