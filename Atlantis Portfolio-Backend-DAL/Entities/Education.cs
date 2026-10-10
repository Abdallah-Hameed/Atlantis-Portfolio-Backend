using System;
using System.Collections.Generic;

namespace Atlantis_Portfolio_Backend_DAL.Entities;

public partial class Education
{
    public int EducationId { get; set; }

    public string Institution { get; set; } = null!;

    public string Degree { get; set; } = null!;

    public string FieldOfStudy { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool IsCurrent { get; set; }

    public short DisplayOrder { get; set; }
}
