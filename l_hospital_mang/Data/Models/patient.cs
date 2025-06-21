using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
//using l_hospital_mang.Migrations;

namespace l_hospital_mang.Data.Models
{
    public class patient
    {
        [Key]
        public long Id { get; set; }
        [Required(ErrorMessage = "The value is required.")]

        public string First_Name { get; set; }
        [Required(ErrorMessage = "The value is required.")]

        public string Middel_name { get; set; }
        [Required(ErrorMessage = "The value is required.")]

        public string Last_Name { get; set; }
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        [DataType(DataType.Date)]
        public DateTime? Age { get; set; }

        public string? Residence { get; set; }
        [RegularExpression(@"^\d{11}$", ErrorMessage = "ID number must be exactly 11 digits.")]
        public string? ID_Number { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^(09\d{8}|011\d{7})$", ErrorMessage = "Phone number must start with 09 or 011 and be exactly 10 digits.")]
        public string PhoneNumber { get; set; }
        [NotMapped]
        public IFormFile? Image { get; set; }

        public string? ImagePath { get; set; }
        public ICollection<Consulting_reservation> Consulting_reservations { get; set; }
        public ICollection<Medical_Health> Medical_Healths { get; set; }

        public virtual ICollection<surgery_reservations> SurgeryReservations { get; set; } = new List<surgery_reservations>();
         [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{10}$", ErrorMessage = "Password must be exactly 10 characters and include at least one uppercase letter, one lowercase letter, one digit, and one special character.")]

        public string PasswordHash { get; set; } = string.Empty;

        public bool? IsVerified { get; set; } = false;

        [StringLength(6, ErrorMessage = "Verification code must be 6 characters.")]
        public string? VerificationCode { get; set; }

        public DateTime? CodeExpiresAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public string? IdentityUserId { get; set; }
        public ICollection<AmbulanceRequest>? AmbulanceRequests { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

    }
}
