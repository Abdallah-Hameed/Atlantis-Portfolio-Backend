using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTO;

public class clsUpdateCourseDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string CourseName { get; set; } = null!;

    [Required]
    [StringLength(150, MinimumLength = 1)]
    public string Provider { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly? CompletionDate { get; set; }

    [StringLength(500)]
    [Url]
    public string? CertificateUrl { get; set; }
}