using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
        [Required(ErrorMessage = "The value is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "The value must be positive.")]
        public int ID_Number { set; get; }
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string PhoneNumber { get; set; }
        [NotMapped]
        public IFormFile Image { get; set; }

        public string ImagePath { get; set; }
        public ICollection<Consulting_reservation> Consulting_reservations { get; set; }
        public ICollection<Medical_Health> Medical_Healths { get; set; }

        public Requests Requests { get; set; }
        public virtual ICollection<surgery_reservations> SurgeryReservations { get; set; } = new List<surgery_reservations>();


    }
}
