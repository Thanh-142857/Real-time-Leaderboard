using System.ComponentModel.DataAnnotations;

namespace Real_time_Leaderboard.DTOs
{
    public class SubmitScoreRequest
    {
        public required int LeaderboardId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Score value must be a positive integer.")]
        public required int ScoreValue { get; set; }
    }
}
