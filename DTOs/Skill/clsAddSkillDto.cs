using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTO;

public class clsAddSkillDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string SkillName { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string SkillCategory { get; set; } = null!;

    [Range(1, 5)]
    public byte SkillLevel { get; set; }
}