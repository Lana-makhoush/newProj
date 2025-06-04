using System.ComponentModel.DataAnnotations;

namespace l_hospital_mang.Data.Models
{
    public class CAmbulance_Car
    {
        public int Id { get; set; }  
        [Required]
        [MaxLength(15)]
        public string CarNumber { get; set; } 
        [Required]
        public bool IsAvailable { get; set; }
        public Requests Request { get; set; }


    }
}
