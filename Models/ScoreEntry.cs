namespace Real_time_Leaderboard.Models
{
    public class ScoreEntry : BaseEntity
    {
        public int ScoreEntryId { get; set; }

        public double Score { get; set; }

        public DateTime SubmittedAt { get; set; }

        public int PlayerId { get; set; }

        public Player Player { get; set; } = null!;

        public int LeaderboardId { get; set; }

        public Leaderboard Leaderboard { get; set; } = null!;
    }
}
