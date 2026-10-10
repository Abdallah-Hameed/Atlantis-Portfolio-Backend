using System;
using System.Collections.Generic;

namespace Atlantis_Portfolio_Backend_DAL.Entities;

public partial class ProjectSkill
{
    public int ProjectSkillId { get; set; }

    public int ProjectId { get; set; }

    public int SkillId { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual Skill Skill { get; set; } = null!;
}
