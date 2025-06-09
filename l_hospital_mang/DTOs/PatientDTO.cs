using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace l_hospital_mang.DTOs
{
    public class PatientDTO
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "The first name is required.")]
        public string First_Name { get; set; }

        [Required(ErrorMessage = "The middle name is required.")]
        public string Middel_name { get; set; }

        [Required(ErrorMessage = "The last name is required.")]
        public string Last_Name { get; set; }

        [Required(ErrorMessage = "Age (date of birth) is required.")]
        [DataType(DataType.Date)]
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateTime? Age { get; set; }

        [Required(ErrorMessage = "Residence is required.")]
        public string Residence { get; set; }

        [Required(ErrorMessage = "ID Number is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "The value must be positive.")]
        public int ID_Number { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string PhoneNumber { get; set; }

        public string? ImagePath { get; set; }

        public IFormFile? Image { get; set; }

        // Optional: Navigation properties if needed
        [JsonIgnore]
        public ICollection<MedicalHealthDTO>? Medical_Healths { get; set; }
    }
}
