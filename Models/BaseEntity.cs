namespace Real_time_Leaderboard.Models
{
    public abstract class BaseEntity 
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        public int? CreatedBy { get; set; }
    }
}
