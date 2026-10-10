using System.ComponentModel.DataAnnotations;

namespace Atlantis_Portfolio_Backend.DTOs.Auth;

public class clsRefreshRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}