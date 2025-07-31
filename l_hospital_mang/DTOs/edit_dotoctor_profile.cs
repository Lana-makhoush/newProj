using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace l_hospital_mang.DTOs
{
    public class edit_dotoctor_profile
    {



        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Overview must contain letters only.")]

        public string? First_Name { get; set; }
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Overview must contain letters only.")]

        public string? Middel_name { get; set; }

        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Overview must contain letters only.")]

        public string? Last_Name { get; set; }

        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Overview must contain letters only.")]

        public string? Residence { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [RegularExpression(@"^(09|011)[0-9]{8}$", ErrorMessage = "Phone number must start with 09 or 011 and be 10 digits long.")]

        public string? PhoneNumber { get; set; }

        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Overview must contain letters only.")]

        public string? Overview { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile? Image { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile? PdfFile { get; set; }
    }


}
