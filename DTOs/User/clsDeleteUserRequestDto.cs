using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTOs.User;

public class clsDeleteUserRequestDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int TargetUserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string MasterPassword { get; set; } = null!;
}