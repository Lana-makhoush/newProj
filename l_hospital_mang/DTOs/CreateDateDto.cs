using System.ComponentModel.DataAnnotations;

namespace l_hospital_mang.DTOs
{
    using System.ComponentModel.DataAnnotations;

    public class CreateDateDto
    {
        [Required(ErrorMessage = "Day name is required")]
        [RegularExpression("^(Sunday|Monday|Tuesday|Wednesday|Thursday|Saturday)$",
     ErrorMessage = "Day must be a valid weekday name excluding Friday")]
        public string Day { get; set; } = string.Empty;

        [Required(ErrorMessage = "Month is required")]
        public int Month { get; set; }

        [Required(ErrorMessage = "Year is required")]
        public int? Year { get; set; }

        [Required(ErrorMessage = "Time of day is required")]
        [RegularExpression(@"^(0?[1-9]|1[0-2]):[0-5][0-9]\s?(AM|PM)$", ErrorMessage = "Time must be in format hh:mm AM or PM")]
        public string TimeOfDay { get; set; } = string.Empty;
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "First name must contain letters only.")]
        public string ReservationType { get; set; }
        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }
    }

}
