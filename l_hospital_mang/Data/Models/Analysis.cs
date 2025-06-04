using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace l_hospital_mang.Data.Models
{
    public class Analysis
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "First name must contain letters only.")]
        public string First_Name { get; set; }

        [Required(ErrorMessage = "Middle name is required.")]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Middle name must contain letters only.")]
        public string Middel_name { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Last name must contain letters only.")]
        public string Last_Name { get; set; }

        [Required(ErrorMessage = "The age is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "The age must be regular.")]
        public int? Age { get; set; }

        [Required(ErrorMessage = "A PDF file is required.")]
        public byte[] PdfFile { get; set; }

        [ForeignKey("Consulting_reservation")]
        public int Consulting_reservationId { get; set; }
        public Consulting_reservation Consulting_reservation { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 100000000000, ErrorMessage = "Price must be between 0.01 and 1,000,000.")]
        public decimal Price { get; set; }
    }
}
