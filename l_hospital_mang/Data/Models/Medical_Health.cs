using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace l_hospital_mang.Data.Models
{
    public class Medical_Health
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Main_Complain { get; set; }
        [Required]
        public string Pathological_story { get; set; }
        [Required]
        public string Clinical_examination { get; set; }
        [Required]
        public string Surveillance { get; set; }
        [Required]
        public string Diagnosis { get; set; }
        [Required]
        public string Treatment { get; set;}
        [Required]
        public string plan { get; set; }
        [Required]
        public string notes { get; set; }
        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        public patient Patient { get; set; }
    }

}
