using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTOs.SocialLink;

public class clsReorderSocialLinkItemDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int SocialLinkId { get; set; }

    [Required]
    [Range(1, short.MaxValue)]
    public short DisplayOrder { get; set; }
}