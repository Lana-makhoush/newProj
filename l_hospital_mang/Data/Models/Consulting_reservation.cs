using System.ComponentModel.DataAnnotations;

namespace l_hospital_mang.Data.Models
{
    public class Consulting_reservation
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Reservation type is required")]
        [StringLength(100, ErrorMessage = "Reservation type can't exceed 100 characters")]
        public string ReservationType { get; set; }


        public decimal Price { get; set; }
        public int PatientId { get; set; }
        public patient Patient { get; set; }
        public Analysis Analysis { get; set; }
        public Radiography Radiography { get; set; }
        public Dates Dates { get; set; }
       //public Requests Request { get; set; }
    }
}
