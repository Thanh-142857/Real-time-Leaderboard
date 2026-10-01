using Real_time_Leaderboard.Models;

namespace Real_time_Leaderboard.Services
{
    public interface IScoreService
    {
        public Task<bool> SubmitScoreAsync(int accountId, int leaderboardId, double scoreValue);

        public Task<bool> UpdateScoreAsync(int scoreEntryId, double scoreValue);

        public Task<bool> DeleteScoreAsync(int scoreEntryId);

        public Task<int> GetLeaderboardIdByScoreEntryIdAsync(int scoreEntryId);

        public Task<DateTime> GetLeaderboardEndTimeAsync(int leaderboardId);

        public Task<DateTime> GetLeaderboardStartTimeAsync(int leaderboardId);

        public Task<List<ScoreEntry>> GetScoreEntriesByPlayerIdAsync(int playerId, DateTime? startDate = null, DateTime? endDate = null);

        public Task<string> GetPlayerNameByPlayerId(int playerId);

        public Task<int?> GetPlayerIdUsingAccountIdAsync(int accountId);
    }
}
