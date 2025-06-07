using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace l_hospital_mang.Data.Models
{
    public class Advertisments
    {
        public long Id { get; set; }  
        [Required]
        [MaxLength(100)]  
        public string ServiceName { get; set; }  

        [Range(0, 100)]  
        public decimal DiscountDegree { get; set; }
        [ForeignKey("Clinicscs")]
        public long ClinicId { get; set; }

        public Clinicscs Clinic { get; set; }
    }
}
