using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace l_hospital_mang.Data.Models
{
    public class CAmbulance_Car
    {
        public long Id { get; set; }

        [Required]
        [MaxLength(15)]
        public string CarNumber { get; set; }

        [Required]
        public bool IsAvailable { get; set; }

        public ICollection<AmbulanceRequest> Requests { get; set; }
    }
}
