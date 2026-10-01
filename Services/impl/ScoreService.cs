using Microsoft.EntityFrameworkCore;
using Real_time_Leaderboard.Models;
namespace Real_time_Leaderboard.Services.impl
{
    public class ScoreService : IScoreService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public ScoreService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<int?> GetPlayerIdUsingAccountIdAsync(int accountId)
        {
            return await _context.Players
                .Where(a => a.AccountId == accountId)
                .Select(a => (int?)a.PlayerId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> SubmitScoreAsync(int accountId, int leaderboardId, double scoreValue)
        {
            int? playerId = await GetPlayerIdUsingAccountIdAsync(accountId);

            if (!playerId.HasValue)
            {
                throw new InvalidOperationException($"No player found for account ID {accountId}");
            }
            var scoreEntry = new ScoreEntry
            {
                PlayerId = playerId.Value,
                LeaderboardId = leaderboardId,
                Score = scoreValue,
                SubmittedAt = DateTime.UtcNow
            };
            _context.ScoreEntries.Add(scoreEntry);
            var rowsAffected = await _context.SaveChangesAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateScoreAsync(int scoreEntryId, double scoreValue)
        {
            var score = _context.ScoreEntries.Where(a => a.ScoreEntryId == scoreEntryId).FirstOrDefault();

            if (score != null)
            {
                score.Score = scoreValue;
                var rowsAffected = await _context.SaveChangesAsync();
                return rowsAffected > 0;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> DeleteScoreAsync(int scoreEntryId)
        {
            var score = _context.ScoreEntries.Where(a => a.ScoreEntryId == scoreEntryId).FirstOrDefault();

            if (score != null)
            {
                _context.ScoreEntries.Remove(score);
                var rowsAffected = await _context.SaveChangesAsync();
                return rowsAffected > 0;
            }
            else
            {
                return false;
            }
        }

        public async Task<int> GetLeaderboardIdByScoreEntryIdAsync(int scoreEntryId)
        {
            var scoreEntry = await _context.ScoreEntries
                .Where(se => se.ScoreEntryId == scoreEntryId)
                .FirstOrDefaultAsync();
            if (scoreEntry != null)
            {
                return scoreEntry.LeaderboardId;
            }
            else
            {
                throw new Exception($"Score entry with ID {scoreEntryId} not found.");
            }
        }

        public async Task<DateTime> GetLeaderboardEndTimeAsync(int leaderboardId)
        {
            var leaderboard = await _context.Leaderboards.Where(l => l.LeaderboardId == leaderboardId).FirstOrDefaultAsync();

            if (leaderboard == null)
                throw new Exception($"Leaderboard with ID {leaderboardId} is not found");

            return leaderboard.EndTime;
        }

        public async Task<DateTime> GetLeaderboardStartTimeAsync(int leaderboardId)
        {
            var leaderboard = await _context.Leaderboards.Where(l => l.LeaderboardId == leaderboardId).FirstOrDefaultAsync();

            if (leaderboard == null)
                throw new Exception($"Leaderboard with ID {leaderboardId} is not found");

            return leaderboard.StartTime;
        }

        public async Task<List<ScoreEntry>> GetScoreEntriesByPlayerIdAsync(int playerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ScoreEntries
                .Where(p => p.PlayerId == playerId)
                .AsNoTracking();

            if (startDate.HasValue)
                query = query.Where(e => e.SubmittedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.SubmittedAt <= endDate.Value);

            return await query
                .OrderByDescending(e => e.SubmittedAt)
                .ToListAsync();
        } 

        public async Task<string> GetPlayerNameByPlayerId(int playerId)
        {
            var player = await _context.Players.Where(n => n.PlayerId == playerId).FirstOrDefaultAsync();
            return player.PlayerName ?? "Not Found";
        }
    }
}
