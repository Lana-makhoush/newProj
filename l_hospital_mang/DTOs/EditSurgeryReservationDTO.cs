namespace l_hospital_mang.DTOs
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class EditSurgeryReservationDTO
    {
        public long? DoctorId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? SurgeryDate { get; set; }

        [RegularExpression(@"^(0[1-9]|1[0-2]):[0-5][0-9] (AM|PM)$", ErrorMessage = "SurgeryTime must be in hh:mm AM/PM format.")]
        public string SurgeryTime { get; set; }

        [RegularExpression("^[a-zA-Z ]+$", ErrorMessage = "SurgeryType must contain only letters and spaces.")]
        public string SurgeryType { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal? Price { get; set; }
    }

}
