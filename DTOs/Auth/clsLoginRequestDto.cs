using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTOs.Auth;

public class clsLoginRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Password { get; set; } = null!;
}