using System.ComponentModel.DataAnnotations;

namespace l_hospital_mang.DTOs
{
    public class DoctorProfileUpdateDto
    {
        [Required(ErrorMessage = "Residence is required.")]
        [StringLength(100, ErrorMessage = "Residence cannot exceed 100 characters.")]
        public string Residence { get; set; } = string.Empty;

        [DataType(DataType.Upload)]
        public IFormFile? PdfFile { get; set; }

        [StringLength(100, ErrorMessage = "The overview cannot exceed 100 characters.")]
        public string? Overview { get; set; }
       
    }
}
