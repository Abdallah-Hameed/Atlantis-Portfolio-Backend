using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTOs.Skill;

public class clsReorderSkillItemDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int SkillId { get; set; }

    [Required]
    [Range(1, short.MaxValue)]
    public short DisplayOrder { get; set; }
}