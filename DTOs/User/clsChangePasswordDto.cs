using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTOs.User;

public class clsChangePasswordDto
{
    [Required]
    [MaxLength(100)]
    public string CurrentPassword { get; set; } = null!;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
    public string NewPassword { get; set; } = null!;
}