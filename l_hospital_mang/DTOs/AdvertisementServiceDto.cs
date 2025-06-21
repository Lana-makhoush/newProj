namespace l_hospital_mang.DTOs
{
    using System.ComponentModel.DataAnnotations;

    namespace l_hospital_mang.DTOs
    {
        public class AdvertisementServiceDto
        {
            [Required]
           
            [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "ServiceName must contain letters only.")]

            public string ServiceName { get; set; }

            [Required]
            [Range(10, 100, ErrorMessage = "Discount degree must be between 10 and 100.")]
            public decimal DiscountDegree { get; set; }
        }
    }

}
