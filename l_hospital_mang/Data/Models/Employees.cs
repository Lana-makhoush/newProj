using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace l_hospital_mang.Data.Models
{
    public class Employees
    {
        [Key]
        public long Id { get; set; }
        [Required(ErrorMessage = "The value is required.")]

        public string First_Name { get; set; }
        [Required(ErrorMessage = "The value is required.")]

        public string Middel_name { get; set; }
        [Required(ErrorMessage = "The value is required.")]

        public string Last_Name { get; set; }
        [Required(ErrorMessage = "The value is required.")]
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        [DataType(DataType.Date)]
        public DateTime? Age { get; set; }

        [Required(ErrorMessage = "The value is required.")]
        public string Residence { get; set; }
        [Required(ErrorMessage = "The value is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "The value must be positive.")]
        public int ID_Number { set; get; }
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string PhoneNumber { get; set; }
        [NotMapped]
        public IFormFile Image { get; set; }

        public string ImagePath { get; set; }
        public byte[] PdfFile { get; set; }
        [ForeignKey("TypeId")]
        public long TypeId { get; set; }
        public Type Type { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string PasswordHash { get; set; }

        public bool IsVerified { get; set; } = false;

        public string VerificationCode { get; set; }

        public DateTime? CodeExpiresAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
