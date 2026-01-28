using CryptoMining.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CryptoMining.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<MiningPlan> MiningPlans => Set<MiningPlan>();
        public DbSet<UserMiningPlan> UserMiningPlans => Set<UserMiningPlan>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Deposit> Deposits => Set<Deposit>();
        public DbSet<Withdrawal> Withdrawals => Set<Withdrawal>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Username).IsUnique();
                entity.Property(u => u.WalletBalance).HasPrecision(18, 8);
            });

            // MiningPlan configuration
            modelBuilder.Entity<MiningPlan>(entity =>
            {
                entity.Property(m => m.MinDeposit).HasPrecision(18, 8);
                entity.Property(m => m.MaxDeposit).HasPrecision(18, 8);
                entity.Property(m => m.DailyReturnPercentage).HasPrecision(5, 2);
                entity.Property(m => m.HashRate).HasPrecision(18, 4);
            });

            // UserMiningPlan configuration
            modelBuilder.Entity<UserMiningPlan>(entity =>
            {
                entity.Property(u => u.InvestedAmount).HasPrecision(18, 8);
                entity.Property(u => u.TotalEarned).HasPrecision(18, 8);

                entity.HasOne(u => u.User)
                    .WithMany(u => u.UserMiningPlans)
                    .HasForeignKey(u => u.UserId);

                entity.HasOne(u => u.MiningPlan)
                    .WithMany(m => m.UserMiningPlans)
                    .HasForeignKey(u => u.MiningPlanId);
            });

            // Seed initial mining plans
            modelBuilder.Entity<MiningPlan>().HasData(
                new MiningPlan
                {
                    Id = 1,
                    Name = "Starter",
                    Description = "Perfect for beginners. Start mining with minimal investment.",
                    MinDeposit = 100,
                    MaxDeposit = 999,
                    DailyReturnPercentage = 1.5m,
                    DurationDays = 30,
                    HashRate = 10,
                    IsActive = true
                },
                new MiningPlan
                {
                    Id = 2,
                    Name = "Professional",
                    Description = "For serious miners looking for better returns.",
                    MinDeposit = 1000,
                    MaxDeposit = 9999,
                    DailyReturnPercentage = 2.5m,
                    DurationDays = 60,
                    HashRate = 50,
                    IsActive = true
                },
                new MiningPlan
                {
                    Id = 3,
                    Name = "Enterprise",
                    Description = "Maximum mining power for maximum profits.",
                    MinDeposit = 10000,
                    MaxDeposit = 100000,
                    DailyReturnPercentage = 4.0m,
                    DurationDays = 90,
                    HashRate = 200,
                    IsActive = true
                }
            );
        }
    }

}
