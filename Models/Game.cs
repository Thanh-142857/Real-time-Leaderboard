namespace Real_time_Leaderboard.Models
{
    public class Game : BaseEntity
    {
        public int GameId { get; set; }
        public required string GameName { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();

        public ICollection<Leaderboard> Leaderboard { get; set; } = new List<Leaderboard>();
    }
}
