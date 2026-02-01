namespace CryptoMining.API.Models.DTOs
{
    public class AdminDTOs
    {


        // ============================================
        // MINING PLAN DTOs
        // ============================================

        public record CreateMiningPlanDto(
            string Name,
            string Description,
            decimal MinDeposit,
            decimal MaxDeposit,
            decimal ReturnPercentage,
            int DurationValue,
            string DurationUnit, // "Minutes", "Hours", "Days", "Weeks", "Months"
            decimal HashRate,
            bool IsActive = true
        );

        public record UpdateMiningPlanDto(
            string? Name,
            string? Description,
            decimal? MinDeposit,
            decimal? MaxDeposit,
            decimal? ReturnPercentage,
            int? DurationValue,
            string? DurationUnit,
            decimal? HashRate,
            bool? IsActive
        );

        public record MiningPlanDetailDto(
            int Id,
            string Name,
            string Description,
            decimal MinDeposit,
            decimal MaxDeposit,
            decimal ReturnPercentage,
            int DurationValue,
            string DurationUnit,
            decimal HashRate,
            bool IsActive,
            DateTime CreatedAt,
            DateTime? UpdatedAt,
            int TotalSubscribers,
            decimal TotalInvested
        );

        // ============================================
        // USER DTOs
        // ============================================

        public record CreateUserDto(
            string Username,
            string Email,
            string Password,
            decimal InitialBalance = 0,
            bool IsActive = true,
            bool IsAdmin = false
        );

        public record UpdateUserDto(
            string? Username,
            string? Email,
            string? Password,
            decimal? WalletBalance,
            bool? IsActive,
            bool? IsAdmin
        );

        public record AdminUserDto(
            int Id,
            string Username,
            string Email,
            decimal WalletBalance,
            bool IsActive,
            bool IsAdmin,
            DateTime CreatedAt,
            DateTime? LastLoginAt,
            int ActivePlans,
            decimal TotalDeposited,
            decimal TotalWithdrawn,
            decimal TotalEarnings
        );

        public record UserBalanceAdjustmentDto(
            decimal Amount,
            string Reason,
            TransactionAdjustmentType Type
        );

        public enum TransactionAdjustmentType
        {
            Credit,
            Debit
        }

        // ============================================
        // DEPOSIT DTOs
        // ============================================

        public record CreateDepositDto(
            int UserId,
            decimal Amount,
            string? TransactionHash = null,
            bool AutoConfirm = false
        );

        public record UpdateDepositDto(
            decimal? Amount,
            string? Status,
            string? TransactionHash
        );

        public record AdminDepositDto(
            int Id,
            int UserId,
            string Username,
            string Email,
            decimal Amount,
            string CryptoAddress,
            string? TransactionHash,
            string Status,
            DateTime CreatedAt,
            DateTime? ConfirmedAt
        );

        // ============================================
        // WITHDRAWAL DTOs
        // ============================================

        public record CreateWithdrawalDto(
            int UserId,
            decimal Amount,
            string WalletAddress
        );

        public record UpdateWithdrawalDto(
            decimal? Amount,
            string? Status,
            string? WalletAddress
        );

        public record AdminWithdrawalDto(
            int Id,
            int UserId,
            string Username,
            string Email,
            decimal Amount,
            string WalletAddress,
            string Status,
            DateTime CreatedAt,
            DateTime? ProcessedAt
        );

        // ============================================
        // TRANSACTION DTOs
        // ============================================

        public record AdminTransactionDto(
            int Id,
            int UserId,
            string Username,
            string Type,
            decimal Amount,
            decimal BalanceBefore,
            decimal BalanceAfter,
            string Description,
            string? Reference,
            string? PerformedBy,
            DateTime CreatedAt
        );

        // ============================================
        // DASHBOARD / STATS DTOs
        // ============================================

        public record AdminDashboardDto(
            int TotalUsers,
            int ActiveUsers,
            decimal TotalDeposits,
            decimal PendingDeposits,
            decimal TotalWithdrawals,
            decimal PendingWithdrawals,
            int ActiveMiningPlans,
            decimal TotalMiningInvestment,
            List<RecentActivityDto> RecentActivities
        );

        public record RecentActivityDto(
            string Type,
            string Description,
            decimal? Amount,
            DateTime Timestamp
        );

        // ============================================
        // PAGINATION
        // ============================================

        public record PagedResult<T>(
            List<T> Items,
            int TotalCount,
            int Page,
            int PageSize,
            int TotalPages
        );

        public record PaginationParams(
            int Page = 1,
            int PageSize = 20,
            string? Search = null,
            string? SortBy = null,
            bool SortDescending = true
        );
    }
}
