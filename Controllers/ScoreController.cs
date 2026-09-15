using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Real_time_Leaderboard.DTOs;
using Real_time_Leaderboard.Services;
using System.Security.Claims;

namespace Real_time_Leaderboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoreController : ControllerBase
    {
        private readonly IScoreService _scoreService;

        public ScoreController(IScoreService scoreService)
        {
            _scoreService = scoreService;
        }

        [Authorize]
        [HttpPost("submit-score")]
        public async Task<IActionResult> SubmitScore([FromBody] SubmitScoreRequest request)
        {
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (accountIdClaim == null || roleClaim == null)
            {
                return Unauthorized("User is not authenticated.");
            }
            else if (roleClaim != "Player")
            {
                return Forbid("Only players can submit scores.");
            }
            Console.WriteLine($"here is the account id {int.Parse(accountIdClaim)}");
            var submitted = await _scoreService.SubmitScoreAsync(int.Parse(accountIdClaim), request.LeaderboardId, request.ScoreValue);

            if (!submitted)
                return StatusCode(500, "Failed to submit score.");

            return Ok("Score submitted successfully.");
        }
    }
}
