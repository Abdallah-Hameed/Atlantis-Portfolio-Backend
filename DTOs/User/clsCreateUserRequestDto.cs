using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend;

public class clsCreateUserRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(8)]       // ← أضف
    [MaxLength(100)]     // ← أضف
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
        ErrorMessage = "Password must contain uppercase, lowercase, digit, and special character.")]
    public string Password { get; set; }
}