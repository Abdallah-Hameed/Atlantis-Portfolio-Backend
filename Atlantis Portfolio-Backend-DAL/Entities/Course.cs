using System;
using System.Collections.Generic;

namespace Atlantis_Portfolio_Backend_DAL.Entities;

public partial class Course
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = null!;

    public string Provider { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly? CompletionDate { get; set; }

    public string? CertificateUrl { get; set; }

    public short DisplayOrder { get; set; }
}
