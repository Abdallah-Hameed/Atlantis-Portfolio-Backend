using System.Text.Json.Serialization;

namespace Atlantis_Portfolio_Backend_DAL.Entities;

public partial class Action
{
    public int ActionId { get; set; }

    public string ActionName { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

}