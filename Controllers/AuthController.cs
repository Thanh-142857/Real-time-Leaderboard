using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Real_time_Leaderboard.DTOs;
using Real_time_Leaderboard.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Real_time_Leaderboard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // 1. Check if phone number is already registered
            if (await _context.Accounts.AnyAsync(a => a.PhoneNumber == request.PhoneNumber))
            {
                return BadRequest("An account with this phone number already exists.");
            }

            // 2. Hash password securely
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 3. Create Player profile and Account

            var account = new Account
            {
                PhoneNumber = request.PhoneNumber,
                PasswordHash = passwordHash,
                RoleId = 1, // Default to a standard role ID
                DOB = request.DOB
            };

            var player = new Player
            {
                PlayerName = request.PlayerName,
                Account = account
            };

            _context.Accounts.Add(account);
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful", playerId = player.PlayerId });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Get the Account first (This works for both Gamers and Admins!)
            var account = await _context.Accounts
                .Include(a => a.Role) // You can still include Role because Account has a RoleId
                .FirstOrDefaultAsync(a => a.PhoneNumber == request.PhoneNumber);

            if (account == null) return Unauthorized("Invalid credentials.");

            // 2. Verify the Password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash))
                return Unauthorized("Invalid credentials.");

            // 3. NOW look for the Player profile using the AccountId
            var playerProfile = await _context.Players
                .FirstOrDefaultAsync(p => p.AccountId == account.AccountId);

            // 4. Generate the JWT "Wristband"
            var token = GenerateJwtToken(account);

            // 5. Return the response
            return Ok(new
            {
                message = "Login successful",
                token = token, // The frontend will save this string!
                accountId = account.AccountId,
                // If they have a player profile, use their DisplayName. If not (like an Admin), default to "Admin"
                playerName = playerProfile != null ? playerProfile.PlayerName : "Admin",
                role = account.Role.RoleName
            });
        }

        // Private helper method to create the token
        private string GenerateJwtToken(Account account)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

            // Define the data (Claims) to embed inside the token
            var claims = new List<Claim>
            {
                // ClaimTypes.NameIdentifier is the standard way to store a user ID
                new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                new Claim(ClaimTypes.Role, account.Role.RoleName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Configure expiration, issuer, and the cryptographic signature
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

        [Authorize] // This is the magic attribute that locks the endpoint!
        [HttpGet("my-profile-jwt")]
        public IActionResult GetMyProfile()
        {
            // Extract the claims that we packed into the token earlier
            var accountId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                message = "You have successfully bypassed the login using your token!",
                yourAccountId = accountId,
                yourRole = role
            });
        }
    }
}
