using System.ComponentModel.DataAnnotations;

namespace l_hospital_mang.DTOs
{
    public class CreateDateOptionalDto
    {
        [RegularExpression("^(Sunday|Monday|Tuesday|Wednesday|Thursday|Saturday)$",
      ErrorMessage = "Day must be a valid weekday name excluding Friday")]
        public string? Day { get; set; } = string.Empty;

        public int? Month { get; set; }

        public int? Year { get; set; }

        [RegularExpression(@"^(0?[1-9]|1[0-2]):[0-5][0-9]\s?(AM|PM)$", ErrorMessage = "Time must be in format hh:mm AM or PM")]
        public string? TimeOfDay { get; set; } = string.Empty;
    }
}
