using System.ComponentModel.DataAnnotations;

namespace Archi.API.Models;

public class TacosModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [StringLength(100, ErrorMessage = "{0} must be at most {1} characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(80, ErrorMessage = "{0} must be at most {1} characters")]
    public string? Sauce { get; set; } = string.Empty;

    [Required(ErrorMessage = "{0} is required")]
    [MinLength(3, ErrorMessage = "{0} must be at least {1} characters")]
    [StringLength(100, ErrorMessage = "{0} must be at most {1} characters")]
    public string Meat { get; set; } = string.Empty;

    [Range(0.0, 50.0, ErrorMessage = "Price : {0}, must be between {1} and {2}")]
    public decimal Price { get; set; }
    public bool IsVegan { get; set; }
}