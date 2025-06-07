using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace l_hospital_mang.DTOs
{
    public class ResidentPatientUpdateDto
    {
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "First name must contain letters only.")]
        public string? First_Name { get; set; }

        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Middle name must contain letters only.")]
        public string? Middel_name { get; set; }

        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Last name must contain letters only.")]
        public string? Last_Name { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))]
        [DataType(DataType.Date)]
        public DateTime? Age { get; set; }

        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Residence must contain letters only.")]
        public string? Residence { get; set; }

        [RegularExpression(@"^\d{11}$", ErrorMessage = "ID number must be exactly 11 digits.")]
        public string? ID_Number { get; set; }

        [RegularExpression(@"^(09\d{8}|011\d{7})$", ErrorMessage = "Phone number must start with 09 or 011 and be exactly 10 digits.")]
        public string? PhoneNumber { get; set; }

        public int? RoomId { get; set; }
    }
}
