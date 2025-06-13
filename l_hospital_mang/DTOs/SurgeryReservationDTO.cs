using l_hospital_mang;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class SurgeryReservationDTO
{
    public long? DoctorId { get; set; }

    [Required(ErrorMessage = "SurgeryDate is required.")]
    [JsonConverter(typeof(l_hospital_mang.DateOnlyJsonConverter))]
    public DateTime? SurgeryDate { get; set; } 

    [Required(ErrorMessage = "SurgeryTime is required.")]
    public string SurgeryTime { get; set; }

    [Required(ErrorMessage = "SurgeryType is required.")]
    [RegularExpression("^[a-zA-Z ]+$", ErrorMessage = "SurgeryType must contain only letters and spaces.")]
    public string SurgeryType { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    public decimal Price { get; set; }
}
