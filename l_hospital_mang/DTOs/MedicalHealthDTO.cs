using System.ComponentModel.DataAnnotations;

namespace l_hospital_mang.DTOs
{
    public class MedicalHealthDTO
    {
        public long Id { get; set; }

        [Required]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Main_Complain must contain letters only.")]
        public string Main_Complain { get; set; }

        [Required]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Pathological_story must contain letters only.")]
        public string Pathological_story { get; set; }

        [Required]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Clinical_examination must contain letters only.")]
        public string Clinical_examination { get; set; }

        [Required]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Surveillance must contain letters only.")]
        public string Surveillance { get; set; }

        [Required]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Diagnosis must contain letters only.")]
        public string Diagnosis { get; set; }

        [Required]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Treatment must contain letters only.")]
        public string Treatment { get; set; }

        [Required]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Plan must contain letters only.")]
        public string plan { get; set; }

        [Required]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Notes must contain letters only.")]
        public string notes { get; set; }

        public long? PatientId { get; set; }

        // Optional: Include patient's name if needed for response
        public PatientDTO? Patient { get; set; }
    }
}
