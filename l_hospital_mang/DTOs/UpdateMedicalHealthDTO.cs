using System.ComponentModel.DataAnnotations;

namespace l_hospital_mang.DTOs
{
    public class UpdateMedicalHealthDTO
    {
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Main_Complain must contain letters only.")]
        public string? Main_Complain { get; set; }

        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Pathological_story must contain letters only.")]
        public string? Pathological_story { get; set; }

        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Clinical_examination must contain letters only.")]
        public string? Clinical_examination { get; set; }

        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Surveillance must contain letters only.")]
        public string? Surveillance { get; set; }

        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Diagnosis must contain letters only.")]
        public string? Diagnosis { get; set; }

        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Treatment must contain letters only.")]
        public string? Treatment { get; set; }

        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Plan must contain letters only.")]
        public string? plan { get; set; }

        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Notes must contain letters only.")]
        public string? notes { get; set; }
    }
}
