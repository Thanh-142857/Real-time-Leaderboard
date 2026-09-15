using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Real_time_Leaderboard.DTOs;
using Real_time_Leaderboard.Services;
using System.Security.Claims;

namespace Real_time_Leaderboard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService, IScoreService scoreService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (!result.Succeeded)
                return BadRequest(result.ErrorMessage);

            return Ok($"Registration successful, {result.PlayerId}");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Succeeded)
                return Unauthorized(result.ErrorMessage);

            return Ok(new
            {
                message = "Login successful",
                token = result.Token,
                accountId = result.AccountId,
                playerName = result.PlayerName,
                role = result.Role
            });
        }

        [Authorize]
        [HttpGet("my-profile-jwt")]
        public IActionResult GetMyProfile()
        {
            var accountId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                message = "You have successfully bypassed the login using your token!",
                yourAccountId = accountId,
                yourRole = role
            });
        }

        [HttpPost("verify-phone-number")]
        public async Task<IActionResult> VerifyPhoneNumber([FromBody] VerifyPhoneRequest request)
        {
            var verified = await _authService.VerifyPhoneNumberAsync(request.PhoneNumber, request.Code);

            if (!verified)
                return BadRequest("Invalid or expired verification code.");

            return Ok("Phone number verified.");
        }

        [HttpPost("send-sms-code")]
        public async Task<IActionResult> SendSmsCode([FromBody] SendSmsRequest request)
        {
            var sent = await _authService.SendSmsCodeAsync(request.PhoneNumber);

            if (!sent)
                return StatusCode(502, "Failed to send SMS code.");

            return Ok("SMS code sent.");
        }
    }
}