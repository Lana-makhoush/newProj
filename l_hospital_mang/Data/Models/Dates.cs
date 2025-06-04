using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace l_hospital_mang.Data.Models
{
    public class Dates
    {
        [Key]
        public int Id { get; set; }

        [Range(1, 31, ErrorMessage = "Day must be between 1 and 31")]
        public int Day { get; set; }

        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
        public int Month { get; set; }

        [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Time of day is required")]
        [RegularExpression("^(AM|PM)$", ErrorMessage = "Time of day must be either 'AM' or 'PM'")]
        public string TimeOfDay { get; set; }
        [ForeignKey("Consulting_reservation")]
        public int Consulting_reservationId { get; set; }
        public Consulting_reservation Consulting_reservation { get; set; }
        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }
        public Doctors Doctor { get; set; }
    }
}
