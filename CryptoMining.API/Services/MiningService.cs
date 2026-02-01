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
                    p.ReturnPercentage,
                    p.DurationValue,
                    p.DurationUnit.ToString(),
                    p.HashRate
                ))
                .ToListAsync();
        }

        public async Task<MiningPlanDto?> GetPlanByIdAsync(int id)
        {
            var plan = await _context.MiningPlans.FindAsync(id);
            if (plan == null || !plan.IsActive) return null;

            return new MiningPlanDto(
                plan.Id,
                plan.Name,
                plan.Description,
                plan.MinDeposit,
                plan.MaxDeposit,
                plan.ReturnPercentage,
                plan.DurationValue,
                plan.DurationUnit.ToString(),
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

                if (user == null || plan == null || !plan.IsActive)
                    return null;

                if (dto.Amount < plan.MinDeposit || dto.Amount > plan.MaxDeposit)
                    return null;

                if (user.WalletBalance < dto.Amount)
                    return null;

                var oldBalance = user.WalletBalance;
                user.WalletBalance -= dto.Amount;

                var startDate = DateTime.UtcNow;
                var endDate = plan.CalculateEndDate(startDate);

                var userPlan = new UserMiningPlan
                {
                    UserId = userId,
                    MiningPlanId = dto.MiningPlanId,
                    InvestedAmount = dto.Amount,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = MiningStatus.Active,
                    LastProfitCalculation = startDate
                };

                _context.UserMiningPlans.Add(userPlan);

                _context.Transactions.Add(new Transaction
                {
                    UserId = userId,
                    Type = TransactionType.PlanPurchase,
                    Amount = -dto.Amount,
                    BalanceBefore = oldBalance,
                    BalanceAfter = user.WalletBalance,
                    Description = $"Purchased {plan.Name} mining plan",
                    Reference = $"PLAN-{userPlan.Id}",
                    RelatedEntityId = userPlan.Id
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var expectedReturn = dto.Amount * plan.ReturnPercentage / 100;

                return new UserMiningPlanDto(
                    userPlan.Id,
                    plan.Name,
                    userPlan.InvestedAmount,
                    userPlan.TotalEarned,
                    expectedReturn,
                    userPlan.StartDate,
                    userPlan.EndDate,
                    userPlan.Status.ToString(),
                    plan.HashRate,
                    plan.DurationValue,
                    plan.DurationUnit.ToString()
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
                .OrderByDescending(up => up.StartDate)
                .Select(up => new UserMiningPlanDto(
                    up.Id,
                    up.MiningPlan.Name,
                    up.InvestedAmount,
                    up.TotalEarned,
                    up.InvestedAmount * up.MiningPlan.ReturnPercentage / 100,
                    up.StartDate,
                    up.EndDate,
                    up.Status.ToString(),
                    up.MiningPlan.HashRate,
                    up.MiningPlan.DurationValue,
                    up.MiningPlan.DurationUnit.ToString()
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

            var now = DateTime.UtcNow;
            var processedCount = 0;

            foreach (var plan in activePlans)
            {
                try
                {
                    // Check if plan has ended
                    if (now >= plan.EndDate)
                    {
                        await CompletePlanAsync(plan, now);
                        processedCount++;
                        continue;
                    }

                    // Calculate if it's time for profit distribution
                    var intervalMinutes = plan.MiningPlan.GetProfitIntervalMinutes();
                    var minutesSinceLastCalc = (now - plan.LastProfitCalculation).TotalMinutes;

                    if (minutesSinceLastCalc >= intervalMinutes)
                    {
                        // Calculate how many intervals have passed
                        var intervalsPassed = (int)(minutesSinceLastCalc / intervalMinutes);

                        for (int i = 0; i < intervalsPassed; i++)
                        {
                            var profit = CalculateProfit(plan);
                            await AddProfitAsync(plan, profit);
                        }

                        plan.LastProfitCalculation = now;
                        processedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calculating profit for plan {PlanId}", plan.Id);
                }
            }

            if (processedCount > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Profit calculation completed for {Count} plans", processedCount);
            }
        }

        private async Task CompletePlanAsync(UserMiningPlan plan, DateTime now)
        {
            plan.Status = MiningStatus.Completed;

            // Calculate any remaining profit
            var intervalMinutes = plan.MiningPlan.GetProfitIntervalMinutes();
            var minutesSinceLastCalc = (plan.EndDate - plan.LastProfitCalculation).TotalMinutes;
            var remainingIntervals = (int)(minutesSinceLastCalc / intervalMinutes);

            var remainingProfit = 0m;
            for (int i = 0; i < remainingIntervals; i++)
            {
                remainingProfit += CalculateProfit(plan);
            }

            // Return principal + remaining profits
            var totalReturn = plan.InvestedAmount + remainingProfit;
            var oldBalance = plan.User.WalletBalance;
            plan.User.WalletBalance += totalReturn;
            plan.TotalEarned += remainingProfit;

            _context.Transactions.Add(new Transaction
            {
                UserId = plan.UserId,
                Type = TransactionType.MiningProfit,
                Amount = totalReturn,
                BalanceBefore = oldBalance,
                BalanceAfter = plan.User.WalletBalance,
                Description = $"Mining plan completed - Principal (${plan.InvestedAmount:F2}) + Final profit (${remainingProfit:F2})",
                Reference = $"PLAN-{plan.Id}-COMPLETE",
                RelatedEntityId = plan.Id
            });

            _logger.LogInformation("Plan {PlanId} completed for user {UserId}. Total return: ${Return:F2}",
                plan.Id, plan.UserId, totalReturn);
        }

        private async Task AddProfitAsync(UserMiningPlan plan, decimal profit)
        {
            var oldBalance = plan.User.WalletBalance;
            plan.User.WalletBalance += profit;
            plan.TotalEarned += profit;

            _context.Transactions.Add(new Transaction
            {
                UserId = plan.UserId,
                Type = TransactionType.MiningProfit,
                Amount = profit,
                BalanceBefore = oldBalance,
                BalanceAfter = plan.User.WalletBalance,
                Description = $"Mining profit from {plan.MiningPlan.Name}",
                Reference = $"PLAN-{plan.Id}-PROFIT",
                RelatedEntityId = plan.Id
            });
        }

        private decimal CalculateProfit(UserMiningPlan plan)
        {
            return plan.InvestedAmount * plan.MiningPlan.ReturnPercentage / 100;
        }
    }
}
