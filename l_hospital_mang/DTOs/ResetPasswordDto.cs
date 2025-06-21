using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace l_hospital_mang.DTOs
{
    public class ResetPasswordDto
    {
        [Required]
        [FromHeader(Name = "Email")]
        public string Email { get; set; }

        [Required]
        public string VerificationCode { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{10}$",
            ErrorMessage = "Password must be exactly 10 characters and include at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string NewPassword { get; set; }
    }
}
