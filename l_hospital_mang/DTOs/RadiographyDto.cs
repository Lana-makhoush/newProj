using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class RadiographyDto
{
    [Required(ErrorMessage = "Image is required.")]
    public IFormFile Image { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, 1000000, ErrorMessage = "Price must be between 0.01 and 1,000,000.")]
    public decimal Price { get; set; }
}
