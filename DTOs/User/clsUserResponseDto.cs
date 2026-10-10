using System.ComponentModel.DataAnnotations;

public class clsUserResponseDto
{
    public int UserId { get; set; }

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public bool IsActive { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }
}