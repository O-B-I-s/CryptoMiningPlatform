namespace CryptoMining.API.Models.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? RelatedMiningPlanId { get; set; }

        public User User { get; set; } = null!;
    }

    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        MiningProfit,
        PlanPurchase,
        Refund
    }
}
