﻿namespace l_hospital_mang.DTOs
{
    using System.ComponentModel.DataAnnotations;

    namespace l_hospital_mang.DTOs
    {
        public class PatientRegisterDto
        {
            [Required(ErrorMessage = "first name  is required.")]

            [RegularExpression("^[A-Za-zأ-ي]+$", ErrorMessage = "First name must contain only letters.")]
            public string First_Name { get; set; }
            [Required(ErrorMessage = "Middle name  is required.")]

            [RegularExpression("^[A-Za-zأ-ي]*$", ErrorMessage = "Middle name must contain only letters.")]
            public string Middel_name { get; set; }
            [Required(ErrorMessage = "Last name  is required.")]

            [RegularExpression("^[A-Za-zأ-ي]+$", ErrorMessage = "Last name must contain only letters.")]
            public string Last_Name { get; set; }

            [Required(ErrorMessage = "Phone number is required.")]
            [Phone(ErrorMessage = "Invalid phone number format.")]
            [RegularExpression(@"^(09|011)[0-9]{8}$", ErrorMessage = "Phone number must start with 09 or 011 and be 10 digits long.")]
            public string PhoneNumber { get; set; }

            [EmailAddress(ErrorMessage = "Invalid email address.")]
            public string Email { get; set; }
            [Required(ErrorMessage = "Password is required.")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{10}$", ErrorMessage = "Password must be exactly 10 characters and include at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
            public string Password { get; set; }

            public bool? IsVerified { get; set; } = false;
            public string? IdentityUserId { get; set; }





        }
    }

}
