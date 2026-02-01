using static CryptoMining.API.Models.DTOs.AdminDTOs;

namespace CryptoMining.API.Services
{
    public interface IAdminService
    {
        // Dashboard
        Task<AdminDashboardDto> GetDashboardAsync();

        // Mining Plans
        Task<PagedResult<MiningPlanDetailDto>> GetMiningPlansAsync(PaginationParams pagination);
        Task<MiningPlanDetailDto?> GetMiningPlanByIdAsync(int id);
        Task<MiningPlanDetailDto> CreateMiningPlanAsync(CreateMiningPlanDto dto);
        Task<MiningPlanDetailDto?> UpdateMiningPlanAsync(int id, UpdateMiningPlanDto dto);
        Task<bool> DeleteMiningPlanAsync(int id);

        // Users
        Task<PagedResult<AdminUserDto>> GetUsersAsync(PaginationParams pagination);
        Task<AdminUserDto?> GetUserByIdAsync(int id);
        Task<AdminUserDto> CreateUserAsync(CreateUserDto dto);
        Task<AdminUserDto?> UpdateUserAsync(int id, UpdateUserDto dto, string? adminUsername = null);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> AdjustUserBalanceAsync(int userId, UserBalanceAdjustmentDto dto, string adminUsername);

        // Deposits
        Task<PagedResult<AdminDepositDto>> GetDepositsAsync(PaginationParams pagination, string? status = null);
        Task<AdminDepositDto?> GetDepositByIdAsync(int id);
        Task<AdminDepositDto> CreateDepositAsync(CreateDepositDto dto, string adminUsername);
        Task<AdminDepositDto?> UpdateDepositAsync(int id, UpdateDepositDto dto, string adminUsername);
        Task<bool> ConfirmDepositAsync(int id, string adminUsername);
        Task<bool> RejectDepositAsync(int id, string reason, string adminUsername);
        Task<bool> DeleteDepositAsync(int id);

        // Withdrawals
        Task<PagedResult<AdminWithdrawalDto>> GetWithdrawalsAsync(PaginationParams pagination, string? status = null);
        Task<AdminWithdrawalDto?> GetWithdrawalByIdAsync(int id);
        Task<AdminWithdrawalDto?> UpdateWithdrawalAsync(int id, UpdateWithdrawalDto dto, string adminUsername);
        Task<bool> ApproveWithdrawalAsync(int id, string adminUsername);
        Task<bool> RejectWithdrawalAsync(int id, string reason, string adminUsername);
        Task<bool> DeleteWithdrawalAsync(int id);

        // Transactions
        Task<PagedResult<AdminTransactionDto>> GetTransactionsAsync(PaginationParams pagination, int? userId = null);
    }
}
