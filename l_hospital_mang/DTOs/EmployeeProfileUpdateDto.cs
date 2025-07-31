using l_hospital_mang;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class EmployeeProfileUpdateDto
{
    [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "First name must contain letters only.")]
    public string? First_Name { get; set; }

    [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Middle name must contain letters only.")]
    public string? Middel_name { get; set; }

    [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Last name must contain letters only.")]
    public string? Last_Name { get; set; }

    [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Residence must contain letters only.")]
    public string? Residence { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format.")]
    [RegularExpression(@"^(09|011)[0-9]{8}$", ErrorMessage = "Phone number must start with 09 or 011 and be 10 digits long.")]
    public string? PhoneNumber { get; set; }

    [JsonConverter(typeof(DateOnlyJsonConverter))]
    [DataType(DataType.Date)]
    public DateTime? Age { get; set; } 
    [Range(0, int.MaxValue, ErrorMessage = "The value must be positive.")]
    public int? ID_Number { get; set; } 
}
