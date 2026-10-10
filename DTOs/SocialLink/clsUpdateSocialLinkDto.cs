using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTO;

public class clsUpdateSocialLinkDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Platform { get; set; } = string.Empty;

    [Required]
    [StringLength(500, MinimumLength = 1)]
    [Url]
    public string Url { get; set; } = string.Empty;

    [Range(1, short.MaxValue)]
    public short DisplayOrder { get; set; }
}