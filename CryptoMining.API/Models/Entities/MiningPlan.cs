namespace CryptoMining.API.Models.Entities
{
    public class MiningPlan
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal MinDeposit { get; set; }
        public decimal MaxDeposit { get; set; }
        public decimal ReturnPercentage { get; set; } // Return percentage per duration unit
        public int DurationValue { get; set; } // e.g., 30
        public DurationUnit DurationUnit { get; set; } // e.g., Minutes, Hours, Days, Months
        public decimal HashRate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<UserMiningPlan> UserMiningPlans { get; set; } = new List<UserMiningPlan>();

        // Helper to calculate end date
        public DateTime CalculateEndDate(DateTime startDate)
        {
            return DurationUnit switch
            {
                DurationUnit.Minutes => startDate.AddMinutes(DurationValue),
                DurationUnit.Hours => startDate.AddHours(DurationValue),
                DurationUnit.Days => startDate.AddDays(DurationValue),
                DurationUnit.Weeks => startDate.AddDays(DurationValue * 7),
                DurationUnit.Months => startDate.AddMonths(DurationValue),
                _ => startDate.AddDays(DurationValue)
            };
        }

        // Helper to get profit calculation interval in minutes
        public int GetProfitIntervalMinutes()
        {
            return DurationUnit switch
            {
                DurationUnit.Minutes => 1,
                DurationUnit.Hours => 60,
                DurationUnit.Days => 1440,      // 24 * 60
                DurationUnit.Weeks => 10080,    // 7 * 24 * 60
                DurationUnit.Months => 43200,   // 30 * 24 * 60 (approximate)
                _ => 1440
            };
        }
    }

    public enum DurationUnit
    {
        Minutes = 1,
        Hours = 2,
        Days = 3,
        Weeks = 4,
        Months = 5
    }
}
