using System;
using System.Collections.Generic;

namespace Atlantis_Portfolio_Backend_DAL.Entities;

public partial class Experience
{
    public int ExperienceId { get; set; }

    public string Company { get; set; } = null!;

    public string Position { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool IsCurrent { get; set; }

    public short DisplayOrder { get; set; }
}
