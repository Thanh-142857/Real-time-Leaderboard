namespace Real_time_Leaderboard.Models
{
    public class Leaderboard : BaseEntity
    {
        public int LeaderboardId { get; set; }

        public required string Title { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool IsActive { get; set; }

        public int GameId { get; set; }

        public Game Game { get; set; } = null!;

        public ICollection<ScoreEntry> ScoreEntries { get; set; } = new List<ScoreEntry>();
    }
}
