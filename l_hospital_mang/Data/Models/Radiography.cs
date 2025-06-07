using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace l_hospital_mang.Data.Models
{
    public class Radiography
    {
        [Key]
        public long Id { get; set; }
        public string? First_Name { get; set; }
        //[Required(ErrorMessage = "The value is required.")]

        public string? Middel_name { get; set; }
        //[Required(ErrorMessage = "The value is required.")]

        public string? Last_Name { get; set; }
        //[Required(ErrorMessage = "The value is required.")]
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        [DataType(DataType.Date)]
        public DateTime? Age { get; set; }
        [Required(ErrorMessage = "The value is required.")]
        [NotMapped]
        public IFormFile Image { get; set; }

        public string ImagePath { get; set; }
        [ForeignKey("Consulting_reservation")]
        public long Consulting_reservationId { get; set; }
        public Consulting_reservation Consulting_reservation { get; set; }
        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 1000000, ErrorMessage = "Price must be between 0.01 and 1,000,000.")]
        public decimal Price { get; set; }


    }
}
