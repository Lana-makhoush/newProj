using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace l_hospital_mang.Data.DTOs
{
    public class AnalysisDto
    {
        [Required(ErrorMessage = "First name is required.")]
        public string First_Name { get; set; }

        [Required(ErrorMessage = "Middle name is required.")]
        public string Middel_name { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string Last_Name { get; set; }

        [Required(ErrorMessage = "Age is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Age must be a positive number.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "PDF file is required.")]
        public IFormFile PdfFile { get; set; }

        [Required(ErrorMessage = "Consulting reservation ID is required.")]
        public int Consulting_reservationId { get; set; }

        [Required(ErrorMessage = "Medical health ID is required.")]
        public int Medical_HealthId { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 1000000, ErrorMessage = "Price must be between 0.01 and 1,000,000.")]
        public decimal Price { get; set; }
    }
}
