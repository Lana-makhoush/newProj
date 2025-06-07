using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace l_hospital_mang.DTOs
{
    public class AnalysisUpdateDtocs
    {
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "First name must contain letters only.")]
        public string? First_Name { get; set; }

        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Middle name must contain letters only.")]
        public string? Middel_name { get; set; }

        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Last name must contain letters only.")]
        public string? Last_Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Age { get; set; }  

        public IFormFile? PdfFile { get; set; }

        [Range(0.01, 100000000000, ErrorMessage = "Price must be between 0.01 and 100,000,000,000.")]
        public decimal? Price { get; set; }
    }
}
