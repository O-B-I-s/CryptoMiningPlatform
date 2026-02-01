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
                entity.Property(m => m.ReturnPercentage).HasPrecision(10, 4);
                entity.Property(m => m.HashRate).HasPrecision(18, 4);
            });

            // UserMiningPlan configuration
            modelBuilder.Entity<UserMiningPlan>(entity =>
            {
                entity.Property(u => u.InvestedAmount).HasPrecision(18, 8);
                entity.Property(u => u.TotalEarned).HasPrecision(18, 8);

                entity.HasOne(u => u.User)
                    .WithMany(u => u.UserMiningPlans)
                    .HasForeignKey(u => u.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.MiningPlan)
                    .WithMany(m => m.UserMiningPlans)
                    .HasForeignKey(u => u.MiningPlanId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Transaction configuration
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(t => t.Amount).HasPrecision(18, 8);
                entity.Property(t => t.BalanceBefore).HasPrecision(18, 8);
                entity.Property(t => t.BalanceAfter).HasPrecision(18, 8);

                entity.HasOne(t => t.User)
                    .WithMany(u => u.Transactions)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Deposit configuration
            modelBuilder.Entity<Deposit>(entity =>
            {
                entity.Property(d => d.Amount).HasPrecision(18, 8);

                entity.HasOne(d => d.User)
                    .WithMany(u => u.Deposits)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Withdrawal configuration
            modelBuilder.Entity<Withdrawal>(entity =>
            {
                entity.Property(w => w.Amount).HasPrecision(18, 8);

                entity.HasOne(w => w.User)
                    .WithMany(u => u.Withdrawals)
                    .HasForeignKey(w => w.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 5,
                    Username = "admin",
                    Email = "admin@cryptomining.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    WalletBalance = 0,
                    IsActive = true,
                    IsAdmin = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed sample mining plans
            modelBuilder.Entity<MiningPlan>().HasData(
                new MiningPlan
                {
                    Id = 1,
                    Name = "Quick Starter",
                    Description = "Fast returns! Get 1% every 10 minutes for 2 hours.",
                    MinDeposit = 50,
                    MaxDeposit = 500,
                    ReturnPercentage = 1.0m,
                    DurationValue = 10,
                    DurationUnit = DurationUnit.Minutes,
                    HashRate = 5,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new MiningPlan
                {
                    Id = 2,
                    Name = "Hourly Miner",
                    Description = "Earn 2.5% every hour for 24 hours.",
                    MinDeposit = 100,
                    MaxDeposit = 2000,
                    ReturnPercentage = 2.5m,
                    DurationValue = 1,
                    DurationUnit = DurationUnit.Hours,
                    HashRate = 15,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new MiningPlan
                {
                    Id = 3,
                    Name = "Daily Grind",
                    Description = "Steady 5% daily returns for 30 days.",
                    MinDeposit = 500,
                    MaxDeposit = 10000,
                    ReturnPercentage = 5.0m,
                    DurationValue = 1,
                    DurationUnit = DurationUnit.Days,
                    HashRate = 50,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new MiningPlan
                {
                    Id = 4,
                    Name = "Weekly Wealth",
                    Description = "15% weekly returns for 4 weeks.",
                    MinDeposit = 1000,
                    MaxDeposit = 50000,
                    ReturnPercentage = 15.0m,
                    DurationValue = 1,
                    DurationUnit = DurationUnit.Weeks,
                    HashRate = 100,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new MiningPlan
                {
                    Id = 5,
                    Name = "Monthly Master",
                    Description = "Premium plan with 50% monthly returns for 3 months.",
                    MinDeposit = 5000,
                    MaxDeposit = 100000,
                    ReturnPercentage = 50.0m,
                    DurationValue = 1,
                    DurationUnit = DurationUnit.Months,
                    HashRate = 500,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }

}
