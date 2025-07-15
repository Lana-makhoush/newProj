using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace l_hospital_mang.Data.Models
{
    public class Dates
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Day name is required")]
        [RegularExpression("^(Sunday|Monday|Tuesday|Wednesday|Thursday|Saturday)$",
            ErrorMessage = "Day must be a valid weekday name excluding Friday")]
        public string Day { get; set; }

        [Required(ErrorMessage = "Month is required")]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
        public int Month { get; set; }

        [Required(ErrorMessage = "Year is required")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Time of day is required")]
        [RegularExpression(@"^(0?[1-9]|1[0-2]):[0-5][0-9]\s?(AM|PM)$", ErrorMessage = "Time must be in format hh:mm AM or PM")]
        public string TimeOfDay { get; set; }

       
        [ForeignKey("Doctor")]
        public long? DoctorId { get; set; }
        public Doctors? Doctor { get; set; }
        public string? ReservationType { get; set; }
        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }
       
    }
}
