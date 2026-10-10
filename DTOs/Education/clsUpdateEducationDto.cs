using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTO;

public class clsUpdateEducationDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Institution { get; set; } = null!;

    [Required]
    [StringLength(150, MinimumLength = 1)]
    public string Degree { get; set; } = null!;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string FieldOfStudy { get; set; } = null!;

    public string? Description { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [Required]
    public bool IsCurrent { get; set; }
}