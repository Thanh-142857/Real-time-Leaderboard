namespace Real_time_Leaderboard.Models
{
    public class Category : BaseEntity
    {
        public int CategoryId { get; set; }

        public required string CategoryName { get; set; }

        public ICollection<Game> Games { get; set; } = new List<Game>();
    }
}
