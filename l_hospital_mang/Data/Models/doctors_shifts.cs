namespace l_hospital_mang.Data.Models
{
    public class doctors_shifts
    {
        public int DoctorId { get; set; }
        public Doctors Doctor { get; set; }

        public int ShiftId { get; set; }
        public Shifts Shift { get; set; }
    }
}
