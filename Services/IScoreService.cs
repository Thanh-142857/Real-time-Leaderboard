namespace Real_time_Leaderboard.Services
{
    public interface IScoreService
    {
        public Task<bool> SubmitScoreAsync(int playerId, int gameId, int scoreValue);
    }
}
