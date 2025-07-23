using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace l_hospital_mang.DTOs
{
    public class AnalysisDTTo
    {
        [Required(ErrorMessage = "First name is required.")]
        public string First_Name { get; set; }

        [Required(ErrorMessage = "Middle name is required.")]
        public string Middel_name { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string Last_Name { get; set; }

        [Required(ErrorMessage = "Age is required.")]
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        [DataType(DataType.Date)]
        public DateTime Age { get; set; }

        [Required(ErrorMessage = "PDF file is required.")]
        public IFormFile PdfFile { get; set; }

        [Required(ErrorMessage = "Consulting reservation ID is required.")]
        public long Consulting_reservationId { get; set; }

        public int? Medical_HealthId { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 1000000, ErrorMessage = "Price must be between 0.01 and 1,000,000.")]
        public decimal Price { get; set; }
        public string? PdfFilePath { get; set; }
    }
}
