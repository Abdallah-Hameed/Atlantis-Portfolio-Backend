using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTO;

public class clsProjectSkillDto
{
    public int ProjectSkillId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int ProjectId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int SkillId { get; set; }

    public string ProjectName { get; set; } = null!;

    public string SkillName { get; set; } = null!;
}