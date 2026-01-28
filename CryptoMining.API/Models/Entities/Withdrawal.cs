namespace CryptoMining.API.Models.Entities
{
    public class Withdrawal
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string WalletAddress { get; set; } = string.Empty;
        public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }

        public User User { get; set; } = null!;
    }

    public enum WithdrawalStatus
    {
        Pending,
        Processing,
        Completed,
        Rejected
    }
}
