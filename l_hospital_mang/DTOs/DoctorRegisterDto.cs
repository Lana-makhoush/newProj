namespace l_hospital_mang.DTOs
{
    public class DoctorRegisterDto
    {
        public string First_Name { get; set; }
        public string Middel_name { get; set; }
        public string Last_Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool? IsVerified { get; set; } = false;

    }

}
