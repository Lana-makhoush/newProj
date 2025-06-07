using System.ComponentModel.DataAnnotations;

namespace l_hospital_mang.Data.Models
{
    public class Clinicscs
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string Clinic_Name { get; set; }
        public ICollection<Doctors>? Doctors { get; set; }
        public Advertisments? Advertisments { get; set; }
    }
}
