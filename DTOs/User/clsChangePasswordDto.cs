using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTOs.User;

public class clsChangePasswordDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Required]
    public string CurrentPassword { get; set; } = null!;

    [Required]
    public string NewPassword { get; set; } = null!;
}