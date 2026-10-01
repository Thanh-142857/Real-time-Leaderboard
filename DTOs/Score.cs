using System.ComponentModel.DataAnnotations;

namespace Real_time_Leaderboard.DTOs
{
    public class SubmitScoreRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Score value must be a positive integer.")]
        public required int LeaderboardId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Score value must be a positive integer.")]
        public required int ScoreValue { get; set; }
    }

    public class DeleteScoreRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Score value must be a positive integer.")]
        public required int ScoreEntryId { get; set; }
    }

    public class UpdateScoreRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Score value must be a positive integer.")]
        public required int ScoreEntryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Score value must be a positive integer.")]
        public required double ScoreValue { get; set; }
    }

    public class GetLeaderBoardRequest()
    {
        [Range(1, int.MaxValue, ErrorMessage = "Score value must be a positive integer.")]
        public required int LeaderboardId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Score value must be a positive integer.")]
        public required int TopN { get; set; }
    }

    public class LeaderboardEntry
    {
        [Range(1, int.MaxValue, ErrorMessage = "Score value must be a positive integer.")]
        public int PlayerId { get; set; }

        public required string PlayerName { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Score value must be a positive integer.")]
        public double Score { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Score value must be a positive integer.")]
        public int Rank { get; set; }
    }

    public class ScoreEntryDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Score value must be a positive integer.")]
        public int ScoreEntryId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Score value must be a positive integer.")]
        public int PlayerId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Score value must be a positive integer.")]
        public int LeaderboardId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Score value must be a positive integer.")]
        public double Score { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
