//using l_hospital_mang.Migrations;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace l_hospital_mang.Data.Models
{
    public class Doctors
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string First_Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Middle name is required.")]
        [StringLength(50, ErrorMessage = "Middle name cannot exceed 50 characters.")]
        public string Middel_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string Last_Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Residence is required.")]
        [StringLength(100, ErrorMessage = "Residence cannot exceed 100 characters.")]
        public string Residence { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [DataType(DataType.Upload)]
        public byte[]? PdfFile { get; set; }

       
        [StringLength(100, ErrorMessage = "The overview cannot exceed 100 characters.")]
        public string? Overview { get; set; }

        public ICollection<Dates>? Dates { get; set; }

        [ForeignKey("Clinic")]
        public long? ClinicId { get; set; }
        public Clinicscs? Clinic { get; set; }

        public ICollection<doctors_shifts>? DoctorShifts { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string PasswordHash { get; set; } = string.Empty;

        public bool? IsVerified { get; set; } = false;

        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{10}$", ErrorMessage = "Password must be exactly 10 characters and include at least one uppercase letter, one lowercase letter, one digit, and one special character.")]

        public string? VerificationCode { get; set; }

        public DateTime? CodeExpiresAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        [JsonIgnore]
        public virtual ICollection<surgery_reservations> SurgeryReservations { get; set; } = new List<surgery_reservations>();

        public string? IdentityUserId { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }


    }
}
