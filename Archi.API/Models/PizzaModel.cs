using System.ComponentModel.DataAnnotations;
using Archi.Api.Models;

namespace Archi.API.Models;


//[Table("Pizza")]
public class PizzaModel : BaseModel
{

    [Required(ErrorMessage = "{0} is required")]
    [StringLength(100, ErrorMessage = "{0} must be at most {1} characters")]
    //[Column("UnNomQuiClaque")]
    public string Name { get; set; } = string.Empty;

    [StringLength(90, ErrorMessage = "{0} must be at most {1} characters")]
    public string? Base { get; set; } = string.Empty;

    [Required(ErrorMessage = "{0} is required")]
    [MinLength(3, ErrorMessage = "{0} must be at least {1} characters")]
    [StringLength(100, ErrorMessage = "{0} must be at most {1} characters")]
    public string Ingredients { get; set; } = string.Empty;

    [Range(0.0, 50.0, ErrorMessage = "Price : {0}, must be between {1} and {2}")]
    public decimal Price { get; set; }
}