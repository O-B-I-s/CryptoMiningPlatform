using System.ComponentModel.DataAnnotations;

namespace CryptoMining.API.Models.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public decimal WalletBalance { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<UserMiningPlan> UserMiningPlans { get; set; } = new List<UserMiningPlan>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();
        public ICollection<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();
    }
}
