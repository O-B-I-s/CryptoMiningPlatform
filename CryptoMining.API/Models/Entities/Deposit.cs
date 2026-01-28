namespace CryptoMining.API.Models.Entities
{
    public class Deposit
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string CryptoAddress { get; set; } = string.Empty;
        public string TransactionHash { get; set; } = string.Empty;
        public DepositStatus Status { get; set; } = DepositStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedAt { get; set; }

        public User User { get; set; } = null!;
    }

    public enum DepositStatus
    {
        Pending,
        Confirmed,
        Failed,
        Cancelled
    }
}
