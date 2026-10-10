using System.ComponentModel.DataAnnotations;

public class clsSetUserActiveDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Required]
    public bool IsActive { get; set; }
}