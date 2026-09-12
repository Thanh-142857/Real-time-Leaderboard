using System.ComponentModel.DataAnnotations;

namespace Real_time_Leaderboard.Validation
{
    public class MustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return value is bool booleanValue && booleanValue;
        }
    }
}
