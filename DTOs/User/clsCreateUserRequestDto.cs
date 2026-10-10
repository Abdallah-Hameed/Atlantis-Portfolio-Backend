using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend;

public class clsCreateUserRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}