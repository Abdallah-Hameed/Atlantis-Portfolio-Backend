using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend;

public class clsUpdatePortfolioProfileDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string FullName { get; set; } = null!;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string ProfessionalTitle { get; set; } = null!;

    [Required]
    public string Bio { get; set; } = null!;

    [StringLength(500)]
    public string? ProfileImageUrl { get; set; }

    [StringLength(255)]
    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }
}