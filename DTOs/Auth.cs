using Real_time_Leaderboard.Attributes;
using Real_time_Leaderboard.Validation;
using System.ComponentModel.DataAnnotations;

namespace Real_time_Leaderboard.DTOs
{
    public class RegisterRequest
    {
        public required string PhoneNumber { get; set; }
        public required string Password { get; set; }
        public required string PlayerName { get; set; }

        [Required]
        [MinimumAge(16)]
        public DateOnly DOB { get; set; }
        [MustBeTrue(ErrorMessage = "You must consent to data processing to create an account.")]
        public bool ConsentForData { get; set; }
    }

    public class LoginRequest
    {
        public required string PhoneNumber { get; set; }
        public required string Password { get; set; }
    }

    public class SendSmsRequest
    {
        public required string PhoneNumber { get; set; }
    }

    public class VerifyPhoneRequest
    {
        public required string PhoneNumber { get; set; }
        public required string Code { get; set; }
    }
}