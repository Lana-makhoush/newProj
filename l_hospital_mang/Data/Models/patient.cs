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
        [Required(ErrorMessage = "The value is required.")]
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        [DataType(DataType.Date)]
        public DateTime? Age { get; set; }

        [Required(ErrorMessage = "The value is required.")]
        public string Residence { get; set; }
        [Required(ErrorMessage = "ID number is required.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "ID number must be exactly 11 digits.")]
        public string ID_Number { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^(09\d{8}|011\d{7})$", ErrorMessage = "Phone number must start with 09 or 011 and be exactly 10 digits.")]
        public string PhoneNumber { get; set; }
        [NotMapped]
        public IFormFile? Image { get; set; }

        public string? ImagePath { get; set; }
        public ICollection<Consulting_reservation> Consulting_reservations { get; set; }
        public ICollection<Medical_Health> Medical_Healths { get; set; }

        public Requests? Requests { get; set; }
        public virtual ICollection<surgery_reservations> SurgeryReservations { get; set; } = new List<surgery_reservations>();


    }
}
