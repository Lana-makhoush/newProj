using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace l_hospital_mang.Data.Models
{
    public class Advertisments
    {
        public long Id { get; set; }  
        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "ServiceName must contain letters only.")]
        public string ServiceName { get; set; }  

       
        [Required]
        [Range(10, 100, ErrorMessage = "Discount degree must be between 0 and 100.")]

        public decimal DiscountDegree { get; set; }
        [ForeignKey("Clinicscs")]
        public long? ClinicId { get; set; }

        public Clinicscs? Clinic { get; set; }
    }
}
