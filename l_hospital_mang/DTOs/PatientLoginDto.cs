using System.ComponentModel.DataAnnotations;

namespace l_hospital_mang.DTOs
{
    public class PatientLoginDto
    {
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}
