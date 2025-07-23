using System.ComponentModel.DataAnnotations;

namespace l_hospital_mang.DTOs
{
    public class DoctorProfileUpdateDto
    {
        [Required(ErrorMessage = "Residence is required.")]
        [StringLength(100, ErrorMessage = "Residence cannot exceed 100 characters.")]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Residence must contain letters only.")]
        public string Residence { get; set; } = string.Empty;

        [Required(ErrorMessage = "PDF file is required.")]
        [DataType(DataType.Upload)]
        public IFormFile PdfFile { get; set; }

        [Required(ErrorMessage = "Overview is required.")]
        [StringLength(100, ErrorMessage = "The overview cannot exceed 100 characters.")]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Overview must contain letters only.")]
        public string Overview { get; set; } = string.Empty;

        public IFormFile? Image { get; set; }  // الصورة ليست إجبارية في التحديث
    }
}
