using l_hospital_mang.Data.Models;
using System.ComponentModel.DataAnnotations;

public class Rooms
{
    [Key]
    public long Id { get; set; }

    [Required(ErrorMessage = "Room number is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Room number must be a positive number.")]
    public int? RoomNumber { get; set; }

    [Required(ErrorMessage = "Floor number is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Floor number must be a positive number.")]
    public int? FloorNumber { get; set; }

    [Required(ErrorMessage = "Beds number is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Beds number must be a positive number.")]
    public int? bedsNumber { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, 1000000000, ErrorMessage = "Price must be between 0.01 and 1,000,000.")]
    public decimal? Price { get; set; }
    [Required(ErrorMessage = "IsOccupied is required.")]

    public String IsOccupied { get; set; }
    public Resident_patients? Resident_patient { get; set; }
}
