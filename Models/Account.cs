using Microsoft.EntityFrameworkCore;
namespace Real_time_Leaderboard.Models
{
    // This tells the database to make the PhoneNumber column strictly unique
    [Index(nameof(PhoneNumber), IsUnique = true)]
    public class Account : BaseEntity
    {
        public int AccountId { get; set; }
        public required string PasswordHash { get; set; }
        public DateOnly DOB { get; set; }
        public required string PhoneNumber { get; set; }

        public int RoleId { get; set; }

        public Role Role { get; set; } = null!;

    }
}
