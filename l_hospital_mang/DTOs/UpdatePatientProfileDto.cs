using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace l_hospital_mang.DTOs
{
    public class UpdatePatientProfileDto
    {
      

        [DataType(DataType.Date)]
        [JsonConverter(typeof(DateOnlyJsonConverter))]

        public DateTime? Age { get; set; }

        [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Name must contain letters only.")]

        public string? Residence { get; set; }

        [RegularExpression(@"^\d{11}$", ErrorMessage = "ID number must be exactly 11 digits.")]
        public string? ID_Number { get; set; }

        [RegularExpression(@"^(09\d{8}|011\d{7})$", ErrorMessage = "Phone number must start with 09 or 011 and be exactly 10 digits.")]
        public string? PhoneNumber { get; set; }

        public IFormFile? Image { get; set; }
    }
}
