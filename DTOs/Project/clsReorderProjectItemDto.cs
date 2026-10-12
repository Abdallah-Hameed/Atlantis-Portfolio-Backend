using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTOs.Project;

public class clsReorderProjectItemDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ProjectId { get; set; }

    [Required]
    [Range(1, short.MaxValue)]
    public short DisplayOrder { get; set; }
}