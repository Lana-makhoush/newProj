using System.ComponentModel.DataAnnotations;

namespace l_hospital_mang.DTOs
{
    public class UpdateConsultingReservationDTO
    {
        [StringLength(100, ErrorMessage = "Reservation type can't exceed 100 characters")]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Reservation type must contain only letters")]
        public string? ReservationType { get; set; }

        [Range(100.01, double.MaxValue, ErrorMessage = "Price must be greater than 100")]
        public decimal? Price { get; set; }
    }
}
