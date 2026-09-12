using System.Security.Principal;

namespace Real_time_Leaderboard.Models
{
    public class Player : BaseEntity
    {
        public int PlayerId { get; set; }
        public required string PlayerName { get; set; }

        public int AccountId { get; set; }
        public Account Account { get; set; } = null!;

    }
}
