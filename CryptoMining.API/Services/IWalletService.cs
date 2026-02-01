using CryptoMining.API.Models.DTOs;
using CryptoMining.API.Models.Entities;
using static CryptoMining.API.Models.DTOs.TransactionDTOs;

namespace CryptoMining.API.Services
{
    public interface IWalletService
    {
        Task<Deposit?> CreateDepositAsync(int userId, DepositRequestDto dto);
        Task<bool> ConfirmDepositAsync(int depositId);
        Task<Withdrawal?> RequestWithdrawalAsync(int userId, WithdrawalRequestDto dto);
        Task<List<TransactionDto>> GetTransactionsAsync(int userId, int page = 1, int pageSize = 20);
        Task<decimal> GetBalanceAsync(int userId);
        Task<List<DepositDto>> GetDepositsAsync(int userId, int page = 1, int pageSize = 20);
        Task<List<WithdrawalDto>> GetWithdrawalsAsync(int userId, int page = 1, int pageSize = 20);
    }
}
