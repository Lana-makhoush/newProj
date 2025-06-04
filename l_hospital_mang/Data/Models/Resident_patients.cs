using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using l_hospital_mang.Data.Models;

public class Resident_patients
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "The first name is required.")]
    [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "First name must contain letters only.")]
    public string First_Name { get; set; }

    [Required(ErrorMessage = "The middle name is required.")]
    [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Middle name must contain letters only.")]
    public string Middel_name { get; set; }

    [Required(ErrorMessage = "The last name is required.")]
    [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Last name must contain letters only.")]
    public string Last_Name { get; set; }

    [Required(ErrorMessage = "The age is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Age must be a positive number.")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Residence is required.")]
    [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Residence must contain letters only.")]
    public string Residence { get; set; }

    [Required(ErrorMessage = "ID number is required.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "ID number must be exactly 11 digits.")]
    public string ID_Number { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^(09\d{8}|011\d{7})$", ErrorMessage = "Phone number must start with 09 or 011 and be exactly 10 digits.")]
    public string PhoneNumber { get; set; }

    public int? RoomId { get; set; }

    [ForeignKey("RoomId")]
    public Rooms? Room { get; set; }

    [NotMapped]
    public ICollection<invoice>? Invoices { get; set; }
}
