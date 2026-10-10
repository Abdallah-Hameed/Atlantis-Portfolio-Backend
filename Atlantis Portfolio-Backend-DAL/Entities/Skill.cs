using System;
using System.Collections.Generic;

namespace Atlantis_Portfolio_Backend_DAL.Entities;

public partial class Skill
{
    public int SkillId { get; set; }

    public string SkillName { get; set; } = null!;

    public string SkillCategory { get; set; } = null!;

    public byte SkillLevel { get; set; }

    public short DisplayOrder { get; set; }

    public virtual ICollection<ProjectSkill> ProjectSkills { get; set; } = new List<ProjectSkill>();
}
