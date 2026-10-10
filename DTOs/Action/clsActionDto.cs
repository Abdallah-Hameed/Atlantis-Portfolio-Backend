using System.ComponentModel.DataAnnotations;

public class clsActionDto
{
    [Required]
    [StringLength(50)]
    public string ActionName { get; set; } = null!;

}