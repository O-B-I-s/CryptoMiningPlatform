namespace CryptoMining.API.Models.Entities
{
    public class UserMiningPlan
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MiningPlanId { get; set; }
        public decimal InvestedAmount { get; set; }
        public decimal TotalEarned { get; set; } = 0;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public MiningStatus Status { get; set; } = MiningStatus.Active;
        public DateTime LastProfitCalculation { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public MiningPlan MiningPlan { get; set; } = null!;
    }

    public enum MiningStatus
    {
        Pending,
        Active,
        Completed,
        Cancelled
    }

}
