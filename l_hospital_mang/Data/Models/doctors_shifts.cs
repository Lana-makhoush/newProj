namespace l_hospital_mang.Data.Models
{
    public class doctors_shifts
    {
        public long DoctorId { get; set; }
        public Doctors Doctor { get; set; }

        public long ShiftId { get; set; }
        public Shifts Shift { get; set; }
    }
}
