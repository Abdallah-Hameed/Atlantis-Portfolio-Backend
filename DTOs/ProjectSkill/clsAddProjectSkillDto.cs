using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTO;

public class clsAddProjectSkillDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ProjectId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int SkillId { get; set; }
}