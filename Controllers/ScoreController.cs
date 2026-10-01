using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Real_time_Leaderboard.DTOs;
using Real_time_Leaderboard.Models;
using Real_time_Leaderboard.Services;
using Real_time_Leaderboard.Services.impl;
using System.Security.Claims;

namespace Real_time_Leaderboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoreController : ControllerBase
    {
        private readonly IScoreService _scoreService;

        private readonly ILeaderboardCacheService _cacheService;

        public ScoreController(IScoreService scoreService, ILeaderboardCacheService leaderboardCacheService)
        {
            _scoreService = scoreService;
            _cacheService = leaderboardCacheService;
        }

        [Authorize]
        [HttpPost("submit-score")]
        public async Task<IActionResult> SubmitScore([FromBody] SubmitScoreRequest request)
        {
            if (!TryGetAccountId(out int accountId))
            {
                return Unauthorized("Account ID claim is missing or invalid.");
            }
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (roleClaim != "Player")
            {
                return StatusCode(403, "Only players can submit scores.");
            }
            var submitted = await _scoreService.SubmitScoreAsync(accountId, request.LeaderboardId, request.ScoreValue);

            if (!submitted)
                return StatusCode(500, "Failed to submit score.");
            await _cacheService.UpdateScoreAsync(request.LeaderboardId, accountId, request.ScoreValue);
            return Ok("Score submitted successfully.");
        }

        [Authorize]
        [HttpPost("delete-score")]
        public async Task<IActionResult> DeleteScore([FromBody] DeleteScoreRequest request)
        {
            if (!TryGetAccountId(out int accountId))
            {
                return Unauthorized("Account ID claim is missing or invalid.");
            }

            int leaderboardId = await _scoreService.GetLeaderboardIdByScoreEntryIdAsync(request.ScoreEntryId);

            var deleted = await _scoreService.DeleteScoreAsync(request.ScoreEntryId);

            if (!deleted)
                return StatusCode(500, "Failed to delete score.");

            await _cacheService.RemovePlayerAsync(leaderboardId, accountId);
            return Ok("Score deleted successfully.");
        }

        [Authorize]
        [HttpPost("update-score")]
        public async Task<IActionResult> UpdateScore([FromBody] UpdateScoreRequest request)
        {
            if (!TryGetAccountId(out int accountId))
            {
                return Unauthorized("Account ID claim is missing or invalid.");
            }

            var updated = await _scoreService.UpdateScoreAsync(request.ScoreEntryId, request.ScoreValue);

            var leaderboardId = await _scoreService.GetLeaderboardIdByScoreEntryIdAsync(request.ScoreEntryId);

            if (!updated)
                return StatusCode(500, "Failed to update score.");
            await _cacheService.UpdateScoreAsync(leaderboardId, accountId, request.ScoreValue);
            return Ok("Score updated successfully.");
        }

        [Authorize]
        [HttpGet("leaderboard/{leaderboardId}")]
        public async Task<IActionResult> GetLeaderboard(int leaderboardId, [FromQuery] int topN = 10)
        {
            if (!TryGetAccountId(out int accountId))
            {
                return Unauthorized("Account ID claim is missing or invalid.");
            }
            topN = Math.Clamp(topN, 1, 100);
            var entries = await _cacheService.GetTopPlayersDtoAsync(leaderboardId, topN);

            return Ok(new
            {
                Scope = "leaderboard",
                LeaderboardId = leaderboardId,
                Period = "alltime",
                TotalPlayers = entries.Count,
                TopPlayers = entries
            });
        }
        [Authorize]
        [HttpGet("score-history/{playerId}")]
        public async Task<IActionResult> GetScoreHistory(int playerId)
        {
            if (!TryGetAccountId(out int accountId))
            {
                return Unauthorized("Account ID claim is missing or invalid.");
            }

            var callerPlayerId = await _scoreService.GetPlayerIdUsingAccountIdAsync(accountId);
            if (!callerPlayerId.HasValue || callerPlayerId.Value != playerId)
            {
                return StatusCode(403, "You can only view your own score history.");
            }

            var scoreEntries = await _scoreService.GetScoreEntriesByPlayerIdAsync(playerId);
            var playerName = await _scoreService.GetPlayerNameByPlayerId(playerId);

            return Ok(new
            {
                Message = $"Player: {playerName}",
                scoreEntries
            });
        }

        [Authorize]
        [HttpGet("leaderboard-time/{leaderboardId}")]
        public async Task<IActionResult> GetLeaderboardTime(int leaderboardId)
        {
            if (!TryGetAccountId(out int accountId))
            {
                return Unauthorized("Account ID claim is missing or invalid.");
            }
            var startTime = await _scoreService.GetLeaderboardStartTimeAsync(leaderboardId);

            var endTime = await _scoreService.GetLeaderboardEndTimeAsync(leaderboardId);

            return Ok(new
            {
                Message = "Leaderboard period retrieved successfully.",
                StartTime = startTime,
                EndTime = endTime
            });
        }

        [Authorize]
        [HttpGet("my-ranking/{leaderboardId}")]
        public async Task<IActionResult> GetPlayerRanking(int leaderboardId)
        {
            if (!TryGetAccountId(out int accountId))
            {
                return Unauthorized("Account ID claim is missing or invalid.");
            }
            var ranking = await _cacheService.GetPlayerRankAsync(leaderboardId, accountId);
            if(ranking is null)
            {
                return StatusCode(404, "You have no score submitted on this leaderboard yet.");
            }
            return Ok($"Your rank is: {ranking}");
        }

        [Authorize]
        [HttpGet("reports/player-scores-period/{playerId}")]
        public async Task<IActionResult> GetPlayerScoresByPeriod(int playerId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (!TryGetAccountId(out int accountId))
            {
                return Unauthorized("Account ID claim is missing or invalid.");
            }

            var callerPlayerId = await _scoreService.GetPlayerIdUsingAccountIdAsync(accountId);
            if (!callerPlayerId.HasValue || callerPlayerId.Value != playerId)
            {
                return StatusCode(403, "You can only view your own score reports.");
            }

            var scoreEntries = await _scoreService.GetScoreEntriesByPlayerIdAsync(playerId, startDate, endDate);

            return Ok(new
            {
                Message = "Player scores retrieved successfully for the specified period.",
                PlayerId = playerId,
                StartDate = startDate,
                EndDate = endDate,
                TotalScores = scoreEntries.Count,
                AverageScore = scoreEntries.Count > 0 ? scoreEntries.Average(e => e.Score) : 0,
                HighestScore = scoreEntries.Count > 0 ? scoreEntries.Max(e => e.Score) : 0,
                LowestScore = scoreEntries.Count > 0 ? scoreEntries.Min(e => e.Score) : 0,
                Scores = scoreEntries
            });
        }

        [Authorize]
        [HttpGet("reports/leaderboard-summary-period/{leaderboardId}")]
        public async Task<IActionResult> GetLeaderboardSummaryByPeriod(int leaderboardId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (!TryGetAccountId(out int accountId))
            {
                return Unauthorized("Account ID claim is missing or invalid.");
            }

            var startTime = await _scoreService.GetLeaderboardStartTimeAsync(leaderboardId);
            var endTime = await _scoreService.GetLeaderboardEndTimeAsync(leaderboardId);
            var topPlayers = await _cacheService.GetTopPlayersDtoAsync(leaderboardId, 100);

            var periodStart = startDate > startTime ? startDate : startTime;
            var periodEnd = endDate < endTime ? endDate : endTime;

            return Ok(new
            {
                Message = "Leaderboard summary retrieved successfully for the specified period.",
                LeaderboardId = leaderboardId,
                LeaderboardStartTime = startTime,
                LeaderboardEndTime = endTime,
                ReportPeriodStart = periodStart,
                ReportPeriodEnd = periodEnd,
                TotalPlayers = topPlayers.Count,
                TopPlayers = topPlayers
            });
        }

        [Authorize]
        [HttpGet("reports/game-top-players/{gameId}")]
        public async Task<IActionResult> GetGameTopPlayers(int gameId, [FromQuery] int topN = 10)
        {
            if (!TryGetAccountId(out int accountId))
            {
                return Unauthorized("Account ID claim is missing or invalid.");
            }
            topN = Math.Clamp(topN, 1, 100);
            var entries = await _cacheService.GetTopPlayersByGameDtoAsync(gameId, topN);

            return Ok(new
            {
                Scope = "game",
                GameId = gameId,
                Period = "alltime",
                TotalPlayers = entries.Count,
                TopPlayers = entries
            });
        }

        private bool TryGetAccountId(out int accountId)
        {
            accountId = 0;
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdClaim) || !int.TryParse(accountIdClaim, out accountId))
            {
                return false;
            }
            return true;
        }
    }
}
