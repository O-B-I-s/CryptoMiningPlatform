namespace CryptoMining.API.Models.Entities
{
    public class MiningPlan
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal MinDeposit { get; set; }
        public decimal MaxDeposit { get; set; }
        public decimal DailyReturnPercentage { get; set; } // e.g., 2.5 for 2.5%
        public int DurationDays { get; set; }
        public decimal HashRate { get; set; } // Simulated hash rate in TH/s
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserMiningPlan> UserMiningPlans { get; set; } = new List<UserMiningPlan>();
    }
}
