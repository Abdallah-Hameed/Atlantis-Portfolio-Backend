using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTOs.SocialLink;

public class clsUpdateSocialLinkDto
{
    [Required]
    [MaxLength(100)]
    public string Platform { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = null!;
}