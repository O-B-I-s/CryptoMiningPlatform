using CryptoMining.API.Data;
using CryptoMining.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static CryptoMining.API.Models.DTOs.MiningPlanAdminDTOs;

namespace CryptoMining.API.Services
{
    public class AdminMiningPlanService : IAdminMiningPlanService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminMiningPlanService> _logger;

        public AdminMiningPlanService(ApplicationDbContext context, ILogger<AdminMiningPlanService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all mining plans with subscriber counts
        /// </summary>
        public async Task<List<MiningPlanListDto>> GetAllPlansAsync(bool includeInactive = true)
        {
            var query = _context.MiningPlans.AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(p => p.IsActive);
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new MiningPlanListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    MinDeposit = p.MinDeposit,
                    MaxDeposit = p.MaxDeposit,
                    ReturnPercentage = p.ReturnPercentage,
                    DurationValue = p.DurationValue,
                    DurationUnit = p.DurationUnit.ToString(),
                    HashRate = p.HashRate,
                    IsActive = p.IsActive,
                    ActiveSubscribers = p.UserMiningPlans.Count(up => up.Status == MiningStatus.Active)
                })
                .ToListAsync();
        }

        /// <summary>
        /// Get detailed mining plan by ID
        /// </summary>
        public async Task<MiningPlanDetailDto?> GetPlanByIdAsync(int id)
        {
            var plan = await _context.MiningPlans
                .Include(p => p.UserMiningPlans)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null)
                return null;

            return new MiningPlanDetailDto
            {
                Id = plan.Id,
                Name = plan.Name,
                Description = plan.Description,
                MinDeposit = plan.MinDeposit,
                MaxDeposit = plan.MaxDeposit,
                ReturnPercentage = plan.ReturnPercentage,
                DurationValue = plan.DurationValue,
                DurationUnit = plan.DurationUnit.ToString(),
                HashRate = plan.HashRate,
                IsActive = plan.IsActive,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt,
                TotalSubscribers = plan.UserMiningPlans.Count,
                ActiveSubscribers = plan.UserMiningPlans.Count(up => up.Status == MiningStatus.Active),
                TotalInvested = plan.UserMiningPlans.Sum(up => up.InvestedAmount),
                TotalProfitPaid = plan.UserMiningPlans.Sum(up => up.TotalEarned)
            };
        }

        /// <summary>
        /// Create a new mining plan
        /// </summary>
        public async Task<MiningPlanDetailDto> CreatePlanAsync(CreateMiningPlanDto dto)
        {
            // Validate duration unit
            if (!Enum.TryParse<DurationUnit>(dto.DurationUnit, ignoreCase: true, out var durationUnit))
            {
                throw new ArgumentException($"Invalid duration unit: {dto.DurationUnit}. Valid values are: Minutes, Hours, Days, Weeks, Months");
            }

            // Validate values
            ValidatePlanData(dto.Name, dto.MinDeposit, dto.MaxDeposit, dto.ReturnPercentage, dto.DurationValue, dto.HashRate);

            var plan = new MiningPlan
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim() ?? string.Empty,
                MinDeposit = dto.MinDeposit,
                MaxDeposit = dto.MaxDeposit,
                ReturnPercentage = dto.ReturnPercentage,
                DurationValue = dto.DurationValue,
                DurationUnit = durationUnit,
                HashRate = dto.HashRate,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.MiningPlans.Add(plan);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Mining plan created: {PlanName} (ID: {PlanId})", plan.Name, plan.Id);

            return (await GetPlanByIdAsync(plan.Id))!;
        }

        /// <summary>
        /// Update an existing mining plan
        /// </summary>
        public async Task<MiningPlanDetailDto?> UpdatePlanAsync(int id, UpdateMiningPlanDto dto)
        {
            var plan = await _context.MiningPlans.FindAsync(id);
            if (plan == null)
                return null;

            // Update only provided fields
            if (!string.IsNullOrWhiteSpace(dto.Name))
                plan.Name = dto.Name.Trim();

            if (dto.Description != null)
                plan.Description = dto.Description.Trim();

            if (dto.MinDeposit.HasValue)
            {
                if (dto.MinDeposit.Value <= 0)
                    throw new ArgumentException("Min deposit must be greater than 0");
                plan.MinDeposit = dto.MinDeposit.Value;
            }

            if (dto.MaxDeposit.HasValue)
            {
                if (dto.MaxDeposit.Value <= 0)
                    throw new ArgumentException("Max deposit must be greater than 0");
                plan.MaxDeposit = dto.MaxDeposit.Value;
            }

            if (dto.ReturnPercentage.HasValue)
            {
                if (dto.ReturnPercentage.Value <= 0)
                    throw new ArgumentException("Return percentage must be greater than 0");
                plan.ReturnPercentage = dto.ReturnPercentage.Value;
            }

            if (dto.DurationValue.HasValue)
            {
                if (dto.DurationValue.Value <= 0)
                    throw new ArgumentException("Duration value must be greater than 0");
                plan.DurationValue = dto.DurationValue.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.DurationUnit))
            {
                if (!Enum.TryParse<DurationUnit>(dto.DurationUnit, ignoreCase: true, out var durationUnit))
                    throw new ArgumentException($"Invalid duration unit: {dto.DurationUnit}");
                plan.DurationUnit = durationUnit;
            }

            if (dto.HashRate.HasValue)
            {
                if (dto.HashRate.Value < 0)
                    throw new ArgumentException("Hash rate cannot be negative");
                plan.HashRate = dto.HashRate.Value;
            }

            if (dto.IsActive.HasValue)
                plan.IsActive = dto.IsActive.Value;

            // Validate min <= max after updates
            if (plan.MinDeposit > plan.MaxDeposit)
                throw new ArgumentException("Min deposit cannot be greater than max deposit");

            plan.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Mining plan updated: {PlanName} (ID: {PlanId})", plan.Name, plan.Id);

            return await GetPlanByIdAsync(id);
        }

        /// <summary>
        /// Delete a mining plan
        /// </summary>
        public async Task<(bool Success, string Message)> DeletePlanAsync(int id)
        {
            var plan = await _context.MiningPlans
                .Include(p => p.UserMiningPlans)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null)
                return (false, "Mining plan not found");

            // Check for active subscriptions
            var activeSubscriptions = plan.UserMiningPlans.Count(up => up.Status == MiningStatus.Active);

            if (activeSubscriptions > 0)
            {
                // Soft delete - just deactivate
                plan.IsActive = false;
                plan.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogWarning("Mining plan {PlanId} deactivated (has {Count} active subscriptions)", id, activeSubscriptions);
                return (true, $"Plan deactivated (has {activeSubscriptions} active subscriptions). Will be fully deleted when all subscriptions complete.");
            }

            // Check for any historical subscriptions
            var totalSubscriptions = plan.UserMiningPlans.Count;

            if (totalSubscriptions > 0)
            {
                // Has history, soft delete
                plan.IsActive = false;
                plan.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Mining plan {PlanId} deactivated (has historical subscriptions)", id);
                return (true, "Plan deactivated. Cannot permanently delete plans with subscription history.");
            }

            // No subscriptions, safe to hard delete
            _context.MiningPlans.Remove(plan);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Mining plan permanently deleted: {PlanId}", id);
            return (true, "Plan permanently deleted");
        }

        /// <summary>
        /// Toggle plan active status
        /// </summary>
        public async Task<bool> TogglePlanStatusAsync(int id)
        {
            var plan = await _context.MiningPlans.FindAsync(id);
            if (plan == null)
                return false;

            plan.IsActive = !plan.IsActive;
            plan.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Mining plan {PlanId} status toggled to {Status}", id, plan.IsActive ? "Active" : "Inactive");
            return true;
        }

        /// <summary>
        /// Get overall mining plan statistics
        /// </summary>
        public async Task<MiningPlanStatsDto> GetPlanStatisticsAsync()
        {
            var plans = await _context.MiningPlans
                .Include(p => p.UserMiningPlans)
                .ToListAsync();

            return new MiningPlanStatsDto
            {
                TotalPlans = plans.Count,
                ActivePlans = plans.Count(p => p.IsActive),
                InactivePlans = plans.Count(p => !p.IsActive),
                TotalActiveSubscriptions = plans.SelectMany(p => p.UserMiningPlans).Count(up => up.Status == MiningStatus.Active),
                TotalInvestedAmount = plans.SelectMany(p => p.UserMiningPlans).Sum(up => up.InvestedAmount),
                TotalProfitPaid = plans.SelectMany(p => p.UserMiningPlans).Sum(up => up.TotalEarned)
            };
        }

        /// <summary>
        /// Validate plan data
        /// </summary>
        private void ValidatePlanData(string name, decimal minDeposit, decimal maxDeposit, decimal returnPercentage, int durationValue, decimal hashRate)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(name))
                errors.Add("Plan name is required");

            if (minDeposit <= 0)
                errors.Add("Min deposit must be greater than 0");

            if (maxDeposit <= 0)
                errors.Add("Max deposit must be greater than 0");

            if (minDeposit > maxDeposit)
                errors.Add("Min deposit cannot be greater than max deposit");

            if (returnPercentage <= 0)
                errors.Add("Return percentage must be greater than 0");

            if (returnPercentage > 1000)
                errors.Add("Return percentage seems too high (max 1000%)");

            if (durationValue <= 0)
                errors.Add("Duration value must be greater than 0");

            if (hashRate < 0)
                errors.Add("Hash rate cannot be negative");

            if (errors.Any())
                throw new ArgumentException(string.Join("; ", errors));
        }
    }
}
