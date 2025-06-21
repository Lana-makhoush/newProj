using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace l_hospital_mang.Data.Models
{
    public class Consulting_reservation
    {
        [Key]
        public long Id { get; set; }
        [Required(ErrorMessage = "Reservation type is required")]
        [StringLength(100, ErrorMessage = "Reservation type can't exceed 100 characters")]
        public string ReservationType { get; set; }
        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }
        public long? PatientId { get; set; }
        public patient Patient { get; set; }
        public Analysis Analysis { get; set; }
        public Radiography Radiography { get; set; }
        [ForeignKey("Date")]
        public long? DateId { get; set; }
        public Dates? Date { get; set; }
      
    }
}
