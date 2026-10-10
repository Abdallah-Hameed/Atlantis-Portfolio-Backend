using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTOs.Auth;

public class clsLoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}