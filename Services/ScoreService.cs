using Real_time_Leaderboard.Models;
namespace Real_time_Leaderboard.Services
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
        public async Task<bool> SubmitScoreAsync(int accountId, int leaderboardId, int scoreValue)
        {
            int? playerId = _context.Players.Where(a => a.AccountId == accountId).Select(a => a.PlayerId).FirstOrDefault();

            if(!playerId.HasValue)
            {
                return false;
            }
            var scoreEntry = new ScoreEntry
            {
                PlayerId = (int)playerId,
                LeaderboardId = leaderboardId,
                Score = scoreValue,
                SubmittedAt = DateTime.UtcNow
            };
            _context.ScoreEntries.Add(scoreEntry);
            var rowsAffected = await _context.SaveChangesAsync();
            return rowsAffected > 0;
        }
    }
}
