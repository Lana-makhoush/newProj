using System.ComponentModel.DataAnnotations.Schema;

namespace l_hospital_mang.Data.Models
{
    public class Requests
    {
        public int Id { get; set; }

        public DateTime OrderTime { get; set; }

        public string PatientLocation { get; set; }  

        public OrderStatus Status { get; set; }
        [ForeignKey("Patient")]
        public int PatientId { get; set; }
        public patient Patient { get; set; }
        //public invoice Invoice { get; set; }
        [ForeignKey("CAmbulance_Car")]
        public int AmbulanceCarId { get; set; }
        public CAmbulance_Car AmbulanceCar { get; set; }
    }

    public enum OrderStatus
    {
        Pending,  
        Completed, 
        Canceled   
    }
}

