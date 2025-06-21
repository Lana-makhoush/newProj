namespace l_hospital_mang.DTOs
{
    using System.ComponentModel.DataAnnotations;

    public class AdvertisementUpdateDto
    {
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "ServiceName must contain letters only.")]

        public string? ServiceName { get; set; }

        [Range(10, 100, ErrorMessage = "DiscountDegree must be between 10 and 100.")]
        public decimal? DiscountDegree { get; set; }
    }

}
