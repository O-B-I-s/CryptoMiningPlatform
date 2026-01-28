using CryptoMining.API.Data;
using CryptoMining.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static CryptoMining.API.Models.DTOs.MiningPlanDTOs;

namespace CryptoMining.API.Services
{
    public class MiningService : IMiningService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MiningService> _logger;

        public MiningService(ApplicationDbContext context, ILogger<MiningService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<MiningPlanDto>> GetAllPlansAsync()
        {
            return await _context.MiningPlans
                .Where(p => p.IsActive)
                .Select(p => new MiningPlanDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.MinDeposit,
                    p.MaxDeposit,
                    p.DailyReturnPercentage,
                    p.DurationDays,
                    p.HashRate
                ))
                .ToListAsync();
        }

        public async Task<MiningPlanDto?> GetPlanByIdAsync(int id)
        {
            var plan = await _context.MiningPlans.FindAsync(id);
            if (plan == null) return null;

            return new MiningPlanDto(
                plan.Id,
                plan.Name,
                plan.Description,
                plan.MinDeposit,
                plan.MaxDeposit,
                plan.DailyReturnPercentage,
                plan.DurationDays,
                plan.HashRate
            );
        }

        public async Task<UserMiningPlanDto?> PurchasePlanAsync(int userId, PurchasePlanDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await _context.Users.FindAsync(userId);
                var plan = await _context.MiningPlans.FindAsync(dto.MiningPlanId);

                if (user == null || plan == null)
                    return null;

                if (dto.Amount < plan.MinDeposit || dto.Amount > plan.MaxDeposit)
                    return null;

                if (user.WalletBalance < dto.Amount)
                    return null;

                // Deduct from wallet
                user.WalletBalance -= dto.Amount;

                // Create mining plan
                var userPlan = new UserMiningPlan
                {
                    UserId = userId,
                    MiningPlanId = dto.MiningPlanId,
                    InvestedAmount = dto.Amount,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(plan.DurationDays),
                    Status = MiningStatus.Active,
                    LastProfitCalculation = DateTime.UtcNow
                };

                _context.UserMiningPlans.Add(userPlan);

                // Record transaction
                var txn = new Transaction
                {
                    UserId = userId,
                    Type = TransactionType.PlanPurchase,
                    Amount = -dto.Amount,
                    Description = $"Purchased {plan.Name} mining plan"
                };

                _context.Transactions.Add(txn);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new UserMiningPlanDto(
                    userPlan.Id,
                    plan.Name,
                    userPlan.InvestedAmount,
                    userPlan.TotalEarned,
                    dto.Amount * plan.DailyReturnPercentage / 100,
                    userPlan.StartDate,
                    userPlan.EndDate,
                    userPlan.Status.ToString(),
                    plan.HashRate
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<UserMiningPlanDto>> GetUserPlansAsync(int userId)
        {
            return await _context.UserMiningPlans
                .Include(up => up.MiningPlan)
                .Where(up => up.UserId == userId)
                .Select(up => new UserMiningPlanDto(
                    up.Id,
                    up.MiningPlan.Name,
                    up.InvestedAmount,
                    up.TotalEarned,
                    up.InvestedAmount * up.MiningPlan.DailyReturnPercentage / 100,
                    up.StartDate,
                    up.EndDate,
                    up.Status.ToString(),
                    up.MiningPlan.HashRate
                ))
                .ToListAsync();
        }

        public async Task CalculateProfitsAsync()
        {
            var activePlans = await _context.UserMiningPlans
                .Include(up => up.MiningPlan)
                .Include(up => up.User)
                .Where(up => up.Status == MiningStatus.Active)
                .ToListAsync();

            foreach (var plan in activePlans)
            {
                // Check if plan has ended
                if (DateTime.UtcNow >= plan.EndDate)
                {
                    plan.Status = MiningStatus.Completed;

                    // Return principal + remaining profits
                    var remainingProfit = CalculateDailyProfit(plan);
                    plan.User.WalletBalance += plan.InvestedAmount + remainingProfit;
                    plan.TotalEarned += remainingProfit;

                    _context.Transactions.Add(new Transaction
                    {
                        UserId = plan.UserId,
                        Type = TransactionType.MiningProfit,
                        Amount = plan.InvestedAmount + remainingProfit,
                        Description = $"Mining plan completed - Principal + Final profit",
                        RelatedMiningPlanId = plan.Id
                    });

                    continue;
                }

                // Calculate time since last profit calculation
                var hoursSinceLastCalc = (DateTime.UtcNow - plan.LastProfitCalculation).TotalHours;

                if (hoursSinceLastCalc >= 24) // Calculate daily
                {
                    var dailyProfit = CalculateDailyProfit(plan);

                    plan.TotalEarned += dailyProfit;
                    plan.User.WalletBalance += dailyProfit;
                    plan.LastProfitCalculation = DateTime.UtcNow;

                    _context.Transactions.Add(new Transaction
                    {
                        UserId = plan.UserId,
                        Type = TransactionType.MiningProfit,
                        Amount = dailyProfit,
                        Description = $"Daily mining profit from {plan.MiningPlan.Name}",
                        RelatedMiningPlanId = plan.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Profit calculation completed for {Count} active plans", activePlans.Count);
        }

        private decimal CalculateDailyProfit(UserMiningPlan plan)
        {
            return plan.InvestedAmount * plan.MiningPlan.DailyReturnPercentage / 100;
        }
    }
}
