using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTO;

public class clsAddProjectDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [StringLength(500)]
    [Url]
    public string? GitHubUrl { get; set; }

    [StringLength(500)]
    [Url]
    public string? LiveUrl { get; set; }
}