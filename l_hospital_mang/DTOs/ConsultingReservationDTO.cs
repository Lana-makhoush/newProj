using System.ComponentModel.DataAnnotations;

namespace l_hospital_mang.DTOs
{
    public class ConsultingReservationDTO
    {
        [Required(ErrorMessage = "Reservation type is required")]
        [StringLength(100, ErrorMessage = "Reservation type can't exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Reservation type must contain letters only")]
        public string ReservationType { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(100.01, double.MaxValue, ErrorMessage = "Price must be greater than 100")]
        public decimal? Price { get; set; }
    }
}
