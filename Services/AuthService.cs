using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Real_time_Leaderboard.DTOs;
using Real_time_Leaderboard.Models;
using RealTimeLeaderboard.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Real_time_Leaderboard.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EsmsService _esmsService;
        private readonly IDistributedCache _cache;

        public AuthService(
            AppDbContext context,
            IConfiguration configuration,
            EsmsService esmsService,
            IDistributedCache cache)
        {
            _context = context;
            _configuration = configuration;
            _esmsService = esmsService;
            _cache = cache;
        }

        public async Task<AuthServiceResult> RegisterAsync(RegisterRequest request)
        {
            var exists = await _context.Accounts
                .AnyAsync(a => a.PhoneNumber == request.PhoneNumber);

            if (exists)
            {
                return new AuthServiceResult
                {
                    Succeeded = false,
                    ErrorMessage = "An account with this phone number already exists."
                };
            }

            // TODO: adjust these field names / defaults to match your actual
            // Account, Player and Role models and RegisterRequest DTO.
            var playerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "Player");

            var account = new Account
            {
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = playerRole!.RoleId
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var player = new Player
            {
                AccountId = account.AccountId,
                PlayerName = request.PlayerName
            };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            return new AuthServiceResult
            {
                Succeeded = true,
                PlayerId = player.PlayerId
            };
        }

        public async Task<LoginServiceResult> LoginAsync(LoginRequest request)
        {
            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.PhoneNumber == request.PhoneNumber);

            if (account == null || !BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash))
            {
                return new LoginServiceResult
                {
                    Succeeded = false,
                    ErrorMessage = "Invalid credentials."
                };
            }

            var playerProfile = await _context.Players
                .FirstOrDefaultAsync(p => p.AccountId == account.AccountId);

            var token = GenerateJwtToken(account);

            return new LoginServiceResult
            {
                Succeeded = true,
                Token = token,
                AccountId = account.AccountId,
                PlayerName = playerProfile != null ? playerProfile.PlayerName : "Admin",
                Role = account.Role.RoleName
            };
        }

        public async Task<bool> SendSmsCodeAsync(string phoneNumber)
        {
            var code = GenerateSmsCode();

            // Stored in Redis (via IDistributedCache) so the code survives app
            // restarts and is visible across multiple instances behind a load balancer.
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(GetOtpCacheKey(phoneNumber), code, cacheOptions);

            return await _esmsService.SendOtpAsync(phoneNumber, code);
        }

        public async Task<bool> VerifyPhoneNumberAsync(string phoneNumber, string code)
        {
            var cachedCode = await _cache.GetStringAsync(GetOtpCacheKey(phoneNumber));

            if (cachedCode != null && cachedCode == code)
            {
                await _cache.RemoveAsync(GetOtpCacheKey(phoneNumber));
                return true;
            }

            return false;
        }

        private string GenerateJwtToken(Account account)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                new Claim(ClaimTypes.Role, account.Role.RoleName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string GenerateSmsCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private static string GetOtpCacheKey(string phoneNumber) => $"otp:{phoneNumber}";
    }
}