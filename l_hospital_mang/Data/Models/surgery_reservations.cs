//using l_hospital_mang.Migrations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace l_hospital_mang.Data.Models
{
    public class surgery_reservations
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long PatientId { get; set; }

        [ForeignKey("PatientId")]
        public virtual patient Patient { get; set; }


        public long? DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctors Doctor { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime SurgeryDate { get; set; }
        public String SurgeryTime { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string SurgeryType { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
        public decimal Price { get; set; }
    }
}