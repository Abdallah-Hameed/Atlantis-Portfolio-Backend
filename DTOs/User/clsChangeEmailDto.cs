using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTOs.User;

public class clsChangeEmailDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Required]
    [EmailAddress]
    public string NewEmail { get; set; } = null!;
}