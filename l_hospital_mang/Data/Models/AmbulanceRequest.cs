using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using l_hospital_mang.Data.Models;

public class AmbulanceRequest
{
    [Key]
    public long Id { get; set; }

    public long PatientId { get; set; }

    [ForeignKey("PatientId")]
    public patient Patient { get; set; } 
    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }

    public DateTime RequestTime { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "Pending";

    public long? CarId { get; set; }

    [ForeignKey("CarId")]
    public CAmbulance_Car Car { get; set; }

    public long? AcceptedByEmployeeId { get; set; }

    [ForeignKey("AcceptedByEmployeeId")]
    public Employees AcceptedByEmployee { get; set; }
}
