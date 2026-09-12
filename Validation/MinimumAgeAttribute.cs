using System.ComponentModel.DataAnnotations;

namespace Real_time_Leaderboard.Attributes
{
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        // Pass the required age into the constructor
        public MinimumAgeAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Check if the value is a DateOnly object
            if (value is DateOnly dob)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                // Add 16 years to their DOB. If that future date is still greater than today, they are too young.
                if (dob.AddYears(_minimumAge) > today)
                {
                    return new ValidationResult($"You must be at least {_minimumAge} years old to register.");
                }

                return ValidationResult.Success;
            }

            // If no date is provided or it's the wrong format, let the [Required] attribute handle it
            return ValidationResult.Success;
        }
    }
}
