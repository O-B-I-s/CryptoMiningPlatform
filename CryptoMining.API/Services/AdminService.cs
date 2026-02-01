using CryptoMining.API.Data;
using CryptoMining.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static CryptoMining.API.Models.DTOs.AdminDTOs;

namespace CryptoMining.API.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminService> _logger;

        public AdminService(ApplicationDbContext context, ILogger<AdminService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // DASHBOARD
        // ============================================

        public async Task<AdminDashboardDto> GetDashboardAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);

            var totalDeposits = await _context.Deposits
                .Where(d => d.Status == DepositStatus.Confirmed)
                .SumAsync(d => d.Amount);

            var pendingDeposits = await _context.Deposits
                .Where(d => d.Status == DepositStatus.Pending)
                .SumAsync(d => d.Amount);

            var totalWithdrawals = await _context.Withdrawals
                .Where(w => w.Status == WithdrawalStatus.Completed)
                .SumAsync(w => w.Amount);

            var pendingWithdrawals = await _context.Withdrawals
                .Where(w => w.Status == WithdrawalStatus.Pending)
                .SumAsync(w => w.Amount);

            var activeMiningPlans = await _context.UserMiningPlans
                .CountAsync(p => p.Status == MiningStatus.Active);

            var totalMiningInvestment = await _context.UserMiningPlans
                .Where(p => p.Status == MiningStatus.Active)
                .SumAsync(p => p.InvestedAmount);

            var recentActivities = await _context.Transactions
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .Select(t => new RecentActivityDto(
                    t.Type.ToString(),
                    t.Description,
                    t.Amount,
                    t.CreatedAt
                ))
                .ToListAsync();

            return new AdminDashboardDto(
                totalUsers,
                activeUsers,
                totalDeposits,
                pendingDeposits,
                totalWithdrawals,
                pendingWithdrawals,
                activeMiningPlans,
                totalMiningInvestment,
                recentActivities
            );
        }

        // ============================================
        // MINING PLANS
        // ============================================

        public async Task<PagedResult<MiningPlanDetailDto>> GetMiningPlansAsync(PaginationParams pagination)
        {
            var query = _context.MiningPlans.AsQueryable();

            if (!string.IsNullOrEmpty(pagination.Search))
            {
                query = query.Where(p => p.Name.Contains(pagination.Search) ||
                                          p.Description.Contains(pagination.Search));
            }

            var totalCount = await query.CountAsync();

            query = pagination.SortBy?.ToLower() switch
            {
                "name" => pagination.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "mindeposit" => pagination.SortDescending ? query.OrderByDescending(p => p.MinDeposit) : query.OrderBy(p => p.MinDeposit),
                "returnpercentage" => pagination.SortDescending ? query.OrderByDescending(p => p.ReturnPercentage) : query.OrderBy(p => p.ReturnPercentage),
                _ => pagination.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
            };

            var items = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(p => new MiningPlanDetailDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.MinDeposit,
                    p.MaxDeposit,
                    p.ReturnPercentage,
                    p.DurationValue,
                    p.DurationUnit.ToString(),
                    p.HashRate,
                    p.IsActive,
                    p.CreatedAt,
                    p.UpdatedAt,
                    p.UserMiningPlans.Count(up => up.Status == MiningStatus.Active),
                    p.UserMiningPlans.Where(up => up.Status == MiningStatus.Active).Sum(up => up.InvestedAmount)
                ))
                .ToListAsync();

            return new PagedResult<MiningPlanDetailDto>(
                items,
                totalCount,
                pagination.Page,
                pagination.PageSize,
                (int)Math.Ceiling(totalCount / (double)pagination.PageSize)
            );
        }

        public async Task<MiningPlanDetailDto?> GetMiningPlanByIdAsync(int id)
        {
            return await _context.MiningPlans
                .Where(p => p.Id == id)
                .Select(p => new MiningPlanDetailDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.MinDeposit,
                    p.MaxDeposit,
                    p.ReturnPercentage,
                    p.DurationValue,
                    p.DurationUnit.ToString(),
                    p.HashRate,
                    p.IsActive,
                    p.CreatedAt,
                    p.UpdatedAt,
                    p.UserMiningPlans.Count(up => up.Status == MiningStatus.Active),
                    p.UserMiningPlans.Where(up => up.Status == MiningStatus.Active).Sum(up => up.InvestedAmount)
                ))
                .FirstOrDefaultAsync();
        }

        public async Task<MiningPlanDetailDto> CreateMiningPlanAsync(CreateMiningPlanDto dto)
        {
            var durationUnit = Enum.Parse<DurationUnit>(dto.DurationUnit, ignoreCase: true);

            var plan = new MiningPlan
            {
                Name = dto.Name,
                Description = dto.Description,
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

            return new MiningPlanDetailDto(
                plan.Id, plan.Name, plan.Description, plan.MinDeposit, plan.MaxDeposit,
                plan.ReturnPercentage, plan.DurationValue, plan.DurationUnit.ToString(),
                plan.HashRate, plan.IsActive, plan.CreatedAt, plan.UpdatedAt, 0, 0
            );
        }

        public async Task<MiningPlanDetailDto?> UpdateMiningPlanAsync(int id, UpdateMiningPlanDto dto)
        {
            var plan = await _context.MiningPlans.FindAsync(id);
            if (plan == null) return null;

            if (dto.Name != null) plan.Name = dto.Name;
            if (dto.Description != null) plan.Description = dto.Description;
            if (dto.MinDeposit.HasValue) plan.MinDeposit = dto.MinDeposit.Value;
            if (dto.MaxDeposit.HasValue) plan.MaxDeposit = dto.MaxDeposit.Value;
            if (dto.ReturnPercentage.HasValue) plan.ReturnPercentage = dto.ReturnPercentage.Value;
            if (dto.DurationValue.HasValue) plan.DurationValue = dto.DurationValue.Value;
            if (dto.DurationUnit != null) plan.DurationUnit = Enum.Parse<DurationUnit>(dto.DurationUnit, ignoreCase: true);
            if (dto.HashRate.HasValue) plan.HashRate = dto.HashRate.Value;
            if (dto.IsActive.HasValue) plan.IsActive = dto.IsActive.Value;

            plan.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Mining plan updated: {PlanName} (ID: {PlanId})", plan.Name, plan.Id);

            return await GetMiningPlanByIdAsync(id);
        }

        public async Task<bool> DeleteMiningPlanAsync(int id)
        {
            var plan = await _context.MiningPlans
                .Include(p => p.UserMiningPlans)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null) return false;

            // Check if there are active subscriptions
            if (plan.UserMiningPlans.Any(up => up.Status == MiningStatus.Active))
            {
                // Soft delete - just deactivate
                plan.IsActive = false;
                plan.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.MiningPlans.Remove(plan);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Mining plan deleted/deactivated: {PlanId}", id);

            return true;
        }

        // ============================================
        // USERS
        // ============================================

        public async Task<PagedResult<AdminUserDto>> GetUsersAsync(PaginationParams pagination)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(pagination.Search))
            {
                query = query.Where(u => u.Username.Contains(pagination.Search) ||
                                          u.Email.Contains(pagination.Search));
            }

            var totalCount = await query.CountAsync();

            query = pagination.SortBy?.ToLower() switch
            {
                "username" => pagination.SortDescending ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username),
                "email" => pagination.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "balance" => pagination.SortDescending ? query.OrderByDescending(u => u.WalletBalance) : query.OrderBy(u => u.WalletBalance),
                _ => pagination.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
            };

            var items = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(u => new AdminUserDto(
                    u.Id,
                    u.Username,
                    u.Email,
                    u.WalletBalance,
                    u.IsActive,
                    u.IsAdmin,
                    u.CreatedAt,
                    u.LastLoginAt,
                    u.UserMiningPlans.Count(p => p.Status == MiningStatus.Active),
                    u.Deposits.Where(d => d.Status == DepositStatus.Confirmed).Sum(d => d.Amount),
                    u.Withdrawals.Where(w => w.Status == WithdrawalStatus.Completed).Sum(w => w.Amount),
                    u.Transactions.Where(t => t.Type == TransactionType.MiningProfit).Sum(t => t.Amount)
                ))
                .ToListAsync();

            return new PagedResult<AdminUserDto>(
                items, totalCount, pagination.Page, pagination.PageSize,
                (int)Math.Ceiling(totalCount / (double)pagination.PageSize)
            );
        }

        public async Task<AdminUserDto?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new AdminUserDto(
                    u.Id,
                    u.Username,
                    u.Email,
                    u.WalletBalance,
                    u.IsActive,
                    u.IsAdmin,
                    u.CreatedAt,
                    u.LastLoginAt,
                    u.UserMiningPlans.Count(p => p.Status == MiningStatus.Active),
                    u.Deposits.Where(d => d.Status == DepositStatus.Confirmed).Sum(d => d.Amount),
                    u.Withdrawals.Where(w => w.Status == WithdrawalStatus.Completed).Sum(w => w.Amount),
                    u.Transactions.Where(t => t.Type == TransactionType.MiningProfit).Sum(t => t.Amount)
                ))
                .FirstOrDefaultAsync();
        }

        public async Task<AdminUserDto> CreateUserAsync(CreateUserDto dto)
        {
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                WalletBalance = dto.InitialBalance,
                IsActive = dto.IsActive,
                IsAdmin = dto.IsAdmin,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            // If initial balance > 0, create transaction
            if (dto.InitialBalance > 0)
            {
                _context.Transactions.Add(new Transaction
                {
                    UserId = user.Id,
                    Type = TransactionType.AdminCredit,
                    Amount = dto.InitialBalance,
                    BalanceBefore = 0,
                    BalanceAfter = dto.InitialBalance,
                    Description = "Initial balance on account creation",
                    PerformedBy = "System"
                });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("User created: {Username} (ID: {UserId})", user.Username, user.Id);

            return (await GetUserByIdAsync(user.Id))!;
        }

        public async Task<AdminUserDto?> UpdateUserAsync(int id, UpdateUserDto dto, string? adminUsername = null)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            var oldBalance = user.WalletBalance;

            if (dto.Username != null) user.Username = dto.Username;
            if (dto.Email != null) user.Email = dto.Email;
            if (dto.Password != null) user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            if (dto.IsActive.HasValue) user.IsActive = dto.IsActive.Value;
            if (dto.IsAdmin.HasValue) user.IsAdmin = dto.IsAdmin.Value;

            // Handle balance change with transaction record
            if (dto.WalletBalance.HasValue && dto.WalletBalance.Value != oldBalance)
            {
                var difference = dto.WalletBalance.Value - oldBalance;
                user.WalletBalance = dto.WalletBalance.Value;

                _context.Transactions.Add(new Transaction
                {
                    UserId = user.Id,
                    Type = difference > 0 ? TransactionType.AdminCredit : TransactionType.AdminDebit,
                    Amount = difference,
                    BalanceBefore = oldBalance,
                    BalanceAfter = dto.WalletBalance.Value,
                    Description = $"Balance adjustment by admin",
                    PerformedBy = adminUsername ?? "Admin"
                });
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User updated: {Username} (ID: {UserId}) by {Admin}",
                user.Username, user.Id, adminUsername);

            return await GetUserByIdAsync(id);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            // Soft delete
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("User deactivated: {UserId}", id);

            return true;
        }

        public async Task<bool> AdjustUserBalanceAsync(int userId, UserBalanceAdjustmentDto dto, string adminUsername)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            var oldBalance = user.WalletBalance;
            var amount = dto.Type == TransactionAdjustmentType.Credit ? dto.Amount : -dto.Amount;
            var newBalance = oldBalance + amount;

            if (newBalance < 0) return false; // Can't go negative

            user.WalletBalance = newBalance;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Transactions.Add(new Transaction
            {
                UserId = userId,
                Type = dto.Type == TransactionAdjustmentType.Credit ? TransactionType.AdminCredit : TransactionType.AdminDebit,
                Amount = amount,
                BalanceBefore = oldBalance,
                BalanceAfter = newBalance,
                Description = dto.Reason,
                PerformedBy = adminUsername,
                Reference = $"ADJ-{DateTime.UtcNow:yyyyMMddHHmmss}"
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation("Balance adjusted for user {UserId}: {Amount} by {Admin}. Reason: {Reason}",
                userId, amount, adminUsername, dto.Reason);

            return true;
        }

        // ============================================
        // DEPOSITS
        // ============================================

        public async Task<PagedResult<AdminDepositDto>> GetDepositsAsync(PaginationParams pagination, string? status = null)
        {
            var query = _context.Deposits.Include(d => d.User).AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<DepositStatus>(status, true, out var depositStatus))
            {
                query = query.Where(d => d.Status == depositStatus);
            }

            if (!string.IsNullOrEmpty(pagination.Search))
            {
                query = query.Where(d => d.User.Username.Contains(pagination.Search) ||
                                          d.User.Email.Contains(pagination.Search) ||
                                          d.CryptoAddress.Contains(pagination.Search));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(d => new AdminDepositDto(
                    d.Id, d.UserId, d.User.Username, d.User.Email,
                    d.Amount, d.CryptoAddress, d.TransactionHash,
                    d.Status.ToString(), d.CreatedAt, d.ConfirmedAt
                ))
                .ToListAsync();

            return new PagedResult<AdminDepositDto>(
                items, totalCount, pagination.Page, pagination.PageSize,
                (int)Math.Ceiling(totalCount / (double)pagination.PageSize)
            );
        }

        public async Task<AdminDepositDto?> GetDepositByIdAsync(int id)
        {
            return await _context.Deposits
                .Include(d => d.User)
                .Where(d => d.Id == id)
                .Select(d => new AdminDepositDto(
                    d.Id, d.UserId, d.User.Username, d.User.Email,
                    d.Amount, d.CryptoAddress, d.TransactionHash,
                    d.Status.ToString(), d.CreatedAt, d.ConfirmedAt
                ))
                .FirstOrDefaultAsync();
        }

        public async Task<AdminDepositDto> CreateDepositAsync(CreateDepositDto dto, string adminUsername)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                throw new ArgumentException("User not found");

            var deposit = new Deposit
            {
                UserId = dto.UserId,
                Amount = dto.Amount,
                CryptoAddress = GenerateDepositAddress(),
                TransactionHash = dto.TransactionHash ?? "",
                Status = dto.AutoConfirm ? DepositStatus.Confirmed : DepositStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ConfirmedAt = dto.AutoConfirm ? DateTime.UtcNow : null
            };

            _context.Deposits.Add(deposit);

            if (dto.AutoConfirm)
            {
                var oldBalance = user.WalletBalance;
                user.WalletBalance += dto.Amount;

                _context.Transactions.Add(new Transaction
                {
                    UserId = dto.UserId,
                    Type = TransactionType.Deposit,
                    Amount = dto.Amount,
                    BalanceBefore = oldBalance,
                    BalanceAfter = user.WalletBalance,
                    Description = $"Deposit confirmed (Admin created)",
                    Reference = $"DEP-{deposit.Id}",
                    PerformedBy = adminUsername,
                    RelatedEntityId = deposit.Id
                });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deposit created by admin {Admin} for user {UserId}: ${Amount}",
                adminUsername, dto.UserId, dto.Amount);

            return (await GetDepositByIdAsync(deposit.Id))!;
        }

        public async Task<AdminDepositDto?> UpdateDepositAsync(int id, UpdateDepositDto dto, string adminUsername)
        {
            var deposit = await _context.Deposits.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);
            if (deposit == null) return null;

            if (dto.Amount.HasValue) deposit.Amount = dto.Amount.Value;
            if (dto.TransactionHash != null) deposit.TransactionHash = dto.TransactionHash;

            if (dto.Status != null && Enum.TryParse<DepositStatus>(dto.Status, true, out var newStatus))
            {
                var oldStatus = deposit.Status;
                deposit.Status = newStatus;

                // Handle status change effects
                if (oldStatus != DepositStatus.Confirmed && newStatus == DepositStatus.Confirmed)
                {
                    deposit.ConfirmedAt = DateTime.UtcNow;
                    var oldBalance = deposit.User.WalletBalance;
                    deposit.User.WalletBalance += deposit.Amount;

                    _context.Transactions.Add(new Transaction
                    {
                        UserId = deposit.UserId,
                        Type = TransactionType.Deposit,
                        Amount = deposit.Amount,
                        BalanceBefore = oldBalance,
                        BalanceAfter = deposit.User.WalletBalance,
                        Description = "Deposit confirmed",
                        Reference = $"DEP-{deposit.Id}",
                        PerformedBy = adminUsername,
                        RelatedEntityId = deposit.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
            return await GetDepositByIdAsync(id);
        }

        public async Task<bool> ConfirmDepositAsync(int id, string adminUsername)
        {
            var deposit = await _context.Deposits.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);
            if (deposit == null || deposit.Status != DepositStatus.Pending) return false;

            deposit.Status = DepositStatus.Confirmed;
            deposit.ConfirmedAt = DateTime.UtcNow;

            var oldBalance = deposit.User.WalletBalance;
            deposit.User.WalletBalance += deposit.Amount;

            _context.Transactions.Add(new Transaction
            {
                UserId = deposit.UserId,
                Type = TransactionType.Deposit,
                Amount = deposit.Amount,
                BalanceBefore = oldBalance,
                BalanceAfter = deposit.User.WalletBalance,
                Description = $"Deposit confirmed",
                Reference = $"DEP-{deposit.Id}",
                PerformedBy = adminUsername,
                RelatedEntityId = deposit.Id
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deposit {DepositId} confirmed by {Admin}", id, adminUsername);
            return true;
        }

        public async Task<bool> RejectDepositAsync(int id, string reason, string adminUsername)
        {
            var deposit = await _context.Deposits.FirstOrDefaultAsync(d => d.Id == id);
            if (deposit == null || deposit.Status != DepositStatus.Pending) return false;

            deposit.Status = DepositStatus.Failed;

            _context.Transactions.Add(new Transaction
            {
                UserId = deposit.UserId,
                Type = TransactionType.Deposit,
                Amount = 0,
                BalanceBefore = 0,
                BalanceAfter = 0,
                Description = $"Deposit rejected: {reason}",
                Reference = $"DEP-{deposit.Id}",
                PerformedBy = adminUsername,
                RelatedEntityId = deposit.Id
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deposit {DepositId} rejected by {Admin}: {Reason}", id, adminUsername, reason);
            return true;
        }

        public async Task<bool> DeleteDepositAsync(int id)
        {
            var deposit = await _context.Deposits.FindAsync(id);
            if (deposit == null) return false;

            // Only allow deletion of pending/failed deposits
            if (deposit.Status == DepositStatus.Confirmed)
                return false;

            _context.Deposits.Remove(deposit);
            await _context.SaveChangesAsync();

            return true;
        }

        // ============================================
        // WITHDRAWALS
        // ============================================

        public async Task<PagedResult<AdminWithdrawalDto>> GetWithdrawalsAsync(PaginationParams pagination, string? status = null)
        {
            var query = _context.Withdrawals.Include(w => w.User).AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<WithdrawalStatus>(status, true, out var withdrawalStatus))
            {
                query = query.Where(w => w.Status == withdrawalStatus);
            }

            if (!string.IsNullOrEmpty(pagination.Search))
            {
                query = query.Where(w => w.User.Username.Contains(pagination.Search) ||
                                          w.User.Email.Contains(pagination.Search) ||
                                          w.WalletAddress.Contains(pagination.Search));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(w => w.CreatedAt)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(w => new AdminWithdrawalDto(
                    w.Id, w.UserId, w.User.Username, w.User.Email,
                    w.Amount, w.WalletAddress, w.Status.ToString(),
                    w.CreatedAt, w.ProcessedAt
                ))
                .ToListAsync();

            return new PagedResult<AdminWithdrawalDto>(
                items, totalCount, pagination.Page, pagination.PageSize,
                (int)Math.Ceiling(totalCount / (double)pagination.PageSize)
            );
        }

        public async Task<AdminWithdrawalDto?> GetWithdrawalByIdAsync(int id)
        {
            return await _context.Withdrawals
                .Include(w => w.User)
                .Where(w => w.Id == id)
                .Select(w => new AdminWithdrawalDto(
                    w.Id, w.UserId, w.User.Username, w.User.Email,
                    w.Amount, w.WalletAddress, w.Status.ToString(),
                    w.CreatedAt, w.ProcessedAt
                ))
                .FirstOrDefaultAsync();
        }

        public async Task<AdminWithdrawalDto?> UpdateWithdrawalAsync(int id, UpdateWithdrawalDto dto, string adminUsername)
        {
            var withdrawal = await _context.Withdrawals.Include(w => w.User).FirstOrDefaultAsync(w => w.Id == id);
            if (withdrawal == null) return null;

            if (dto.WalletAddress != null) withdrawal.WalletAddress = dto.WalletAddress;

            // Handle amount change for pending withdrawals
            if (dto.Amount.HasValue && withdrawal.Status == WithdrawalStatus.Pending)
            {
                var difference = dto.Amount.Value - withdrawal.Amount;

                // Adjust user balance for the difference
                if (withdrawal.User.WalletBalance + withdrawal.Amount - dto.Amount.Value < 0)
                    return null; // Not enough balance for increased amount

                withdrawal.User.WalletBalance -= difference;
                withdrawal.Amount = dto.Amount.Value;
            }

            if (dto.Status != null && Enum.TryParse<WithdrawalStatus>(dto.Status, true, out var newStatus))
            {
                withdrawal.Status = newStatus;
                if (newStatus == WithdrawalStatus.Completed)
                {
                    withdrawal.ProcessedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return await GetWithdrawalByIdAsync(id);
        }

        public async Task<bool> ApproveWithdrawalAsync(int id, string adminUsername)
        {
            var withdrawal = await _context.Withdrawals.Include(w => w.User).FirstOrDefaultAsync(w => w.Id == id);
            if (withdrawal == null || withdrawal.Status != WithdrawalStatus.Pending) return false;

            withdrawal.Status = WithdrawalStatus.Completed;
            withdrawal.ProcessedAt = DateTime.UtcNow;

            // Balance was already deducted when withdrawal was created
            // Just record the completion
            _context.Transactions.Add(new Transaction
            {
                UserId = withdrawal.UserId,
                Type = TransactionType.Withdrawal,
                Amount = -withdrawal.Amount,
                BalanceBefore = withdrawal.User.WalletBalance + withdrawal.Amount,
                BalanceAfter = withdrawal.User.WalletBalance,
                Description = $"Withdrawal completed to {ShortenAddress(withdrawal.WalletAddress)}",
                Reference = $"WTH-{withdrawal.Id}",
                PerformedBy = adminUsername,
                RelatedEntityId = withdrawal.Id
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation("Withdrawal {WithdrawalId} approved by {Admin}", id, adminUsername);
            return true;
        }

        public async Task<bool> RejectWithdrawalAsync(int id, string reason, string adminUsername)
        {
            var withdrawal = await _context.Withdrawals.Include(w => w.User).FirstOrDefaultAsync(w => w.Id == id);
            if (withdrawal == null || withdrawal.Status != WithdrawalStatus.Pending) return false;

            withdrawal.Status = WithdrawalStatus.Rejected;
            withdrawal.ProcessedAt = DateTime.UtcNow;

            // Refund the amount back to user
            var oldBalance = withdrawal.User.WalletBalance;
            withdrawal.User.WalletBalance += withdrawal.Amount;

            _context.Transactions.Add(new Transaction
            {
                UserId = withdrawal.UserId,
                Type = TransactionType.Refund,
                Amount = withdrawal.Amount,
                BalanceBefore = oldBalance,
                BalanceAfter = withdrawal.User.WalletBalance,
                Description = $"Withdrawal rejected: {reason}",
                Reference = $"WTH-{withdrawal.Id}",
                PerformedBy = adminUsername,
                RelatedEntityId = withdrawal.Id
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation("Withdrawal {WithdrawalId} rejected by {Admin}: {Reason}", id, adminUsername, reason);
            return true;
        }

        public async Task<bool> DeleteWithdrawalAsync(int id)
        {
            var withdrawal = await _context.Withdrawals.Include(w => w.User).FirstOrDefaultAsync(w => w.Id == id);
            if (withdrawal == null) return false;

            // Only delete pending - refund the amount
            if (withdrawal.Status == WithdrawalStatus.Pending)
            {
                withdrawal.User.WalletBalance += withdrawal.Amount;
            }

            _context.Withdrawals.Remove(withdrawal);
            await _context.SaveChangesAsync();

            return true;
        }

        // ============================================
        // TRANSACTIONS
        // ============================================

        public async Task<PagedResult<AdminTransactionDto>> GetTransactionsAsync(PaginationParams pagination, int? userId = null)
        {
            var query = _context.Transactions.Include(t => t.User).AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(t => t.UserId == userId.Value);
            }

            if (!string.IsNullOrEmpty(pagination.Search))
            {
                query = query.Where(t => t.User.Username.Contains(pagination.Search) ||
                                          t.Description.Contains(pagination.Search) ||
                                          (t.Reference != null && t.Reference.Contains(pagination.Search)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(t => new AdminTransactionDto(
                    t.Id, t.UserId, t.User.Username, t.Type.ToString(),
                    t.Amount, t.BalanceBefore, t.BalanceAfter,
                    t.Description, t.Reference, t.PerformedBy, t.CreatedAt
                ))
                .ToListAsync();

            return new PagedResult<AdminTransactionDto>(
                items, totalCount, pagination.Page, pagination.PageSize,
                (int)Math.Ceiling(totalCount / (double)pagination.PageSize)
            );
        }

        // ============================================
        // HELPERS
        // ============================================

        private static string GenerateDepositAddress()
        {
            var bytes = new byte[20];
            Random.Shared.NextBytes(bytes);
            return "0x" + Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private static string ShortenAddress(string? address)
        {
            if (string.IsNullOrWhiteSpace(address)) return "Unknown";
            if (address.Length <= 14) return address;
            return $"{address[..6]}...{address[^4..]}";
        }
    }
}
