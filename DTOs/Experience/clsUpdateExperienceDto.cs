using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTO;

public class clsUpdateExperienceDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Company { get; set; } = null!;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Position { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    [Required]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [Required]
    public bool IsCurrent { get; set; }
}