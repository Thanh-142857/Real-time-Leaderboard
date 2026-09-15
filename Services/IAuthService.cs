using Real_time_Leaderboard.DTOs;
using Real_time_Leaderboard.Models;

namespace Real_time_Leaderboard.Services
{
    public interface IAuthService
    {
        Task<AuthServiceResult> RegisterAsync(RegisterRequest request);
        Task<LoginServiceResult> LoginAsync(LoginRequest request);
        Task<bool> SendSmsCodeAsync(string phoneNumber);
        Task<bool> VerifyPhoneNumberAsync(string phoneNumber, string code);
    }

    public class AuthServiceResult
    {
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }
        public int? PlayerId { get; set; }
    }

    public class LoginServiceResult
    {
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Token { get; set; }
        public int AccountId { get; set; }
        public string? PlayerName { get; set; }
        public string? Role { get; set; }
    }
}