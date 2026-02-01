namespace CryptoMining.API.Models.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Reference { get; set; } // e.g., "DEPOSIT-123", "WITHDRAWAL-456"
        public string? PerformedBy { get; set; } // Admin username if admin action
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? RelatedEntityId { get; set; } // Related deposit/withdrawal/plan ID

        public User User { get; set; } = null!;
    }

    public enum TransactionType
    {
        Deposit = 1,
        Withdrawal = 2,
        MiningProfit = 3,
        PlanPurchase = 4,
        Refund = 5,
        AdminCredit = 6,
        AdminDebit = 7,
        Bonus = 8,
        Fee = 9
    }
}
