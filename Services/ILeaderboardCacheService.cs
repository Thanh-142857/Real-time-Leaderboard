using Real_time_Leaderboard.DTOs;
namespace Real_time_Leaderboard.Services
{
    public interface ILeaderboardCacheService
    {

        public Task UpdateScoreAsync(int leaderboardId, int accountId, double score);

        public Task IncrementScoreAsync(int leaderboardId, int accountId, double pointsToAdd);

        public Task<List<KeyValuePair<int, double>>> GetTopPlayersAsync(int leaderboardId, int topN);

        public Task<long?> GetPlayerRankAsync(int leaderboardId, int accountId);

        public Task<double?> GetPlayerScoreAsync(int leaderboardId, int accountId);

        public Task<bool> RemovePlayerAsync(int leaderboardId, int accountId);

        public Task<List<LeaderboardEntry>> GetTopPlayersDtoAsync(int leaderboardId, int topN);

        public Task<List<LeaderboardEntry>> GetTopPlayersByGameDtoAsync(int gameId, int topN);
    }
}
