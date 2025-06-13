using l_hospital_mang;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class a_patient_dto
{
    [Required(ErrorMessage = "The value is required.")]
    [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Name must contain letters only.")]
    public string First_Name { get; set; }

    [Required(ErrorMessage = "The value is required.")]
    [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Name must contain letters only.")]
    public string Middel_name { get; set; }

    [Required(ErrorMessage = "The value is required.")]
    [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Name must contain letters only.")]


    public string Last_Name { get; set; }

    [Required(ErrorMessage = "The value is required.")]
    [JsonConverter(typeof(DateOnlyJsonConverter))]
    [DataType(DataType.Date)]
    public DateTime? Age { get; set; }

    [Required(ErrorMessage = "The value is required.")]
    [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Name must contain letters only.")]

    public string Residence { get; set; }

    [Required(ErrorMessage = "ID number is required.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "ID number must be exactly 11 digits.")]
    public string ID_Number { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^(09\d{8}|011\d{7})$", ErrorMessage = "Phone number must start with 09 or 011 and be exactly 10 digits.")]
    public string PhoneNumber { get; set; }
}
