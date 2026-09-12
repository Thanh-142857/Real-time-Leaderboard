namespace Real_time_Leaderboard.Models
{
    public class Role : BaseEntity
    {
        public int RoleId { get; set; }
        public required string RoleName { get; set; }
        public ICollection<Account> Account { get; set; } = new List<Account>();
    }
}
