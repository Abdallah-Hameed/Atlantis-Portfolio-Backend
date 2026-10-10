using System;
using System.Collections.Generic;

namespace Atlantis_Portfolio_Backend_DAL.Entities;

public partial class Project
{
    public int ProjectId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public string? GitHubUrl { get; set; }

    public string? LiveUrl { get; set; }

    public short DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<ProjectSkill> ProjectSkills { get; set; } = new List<ProjectSkill>();
}
