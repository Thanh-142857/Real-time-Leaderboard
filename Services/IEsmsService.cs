namespace Real_time_Leaderboard.Services
{
    public interface IEsmsService
    {
        public Task<bool> SendOtpAsync(string phoneNumber, string otpCode);
    }
}
