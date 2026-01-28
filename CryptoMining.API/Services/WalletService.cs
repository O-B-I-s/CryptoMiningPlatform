using CryptoMining.API.Data;
using CryptoMining.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static CryptoMining.API.Models.DTOs.TransactionDTOs;

namespace CryptoMining.API.Services
{
    public class WalletService : IWalletService
    {
        private readonly ApplicationDbContext _context;

        public WalletService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Deposit?> CreateDepositAsync(int userId, DepositRequestDto dto)
        {
            var deposit = new Deposit
            {
                UserId = userId,
                Amount = dto.Amount,
                CryptoAddress = GenerateDepositAddress(), // Simulated
                Status = DepositStatus.Pending
            };

            _context.Deposits.Add(deposit);
            await _context.SaveChangesAsync();

            return deposit;
        }

        public async Task<bool> ConfirmDepositAsync(int depositId)
        {
            var deposit = await _context.Deposits
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == depositId);

            if (deposit == null || deposit.Status != DepositStatus.Pending)
                return false;

            deposit.Status = DepositStatus.Confirmed;
            deposit.ConfirmedAt = DateTime.UtcNow;
            deposit.User.WalletBalance += deposit.Amount;

            _context.Transactions.Add(new Transaction
            {
                UserId = deposit.UserId,
                Type = TransactionType.Deposit,
                Amount = deposit.Amount,
                Description = "Deposit confirmed"
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Withdrawal?> RequestWithdrawalAsync(int userId, WithdrawalRequestDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.WalletBalance < dto.Amount)
                return null;

            // Deduct immediately (hold)
            user.WalletBalance -= dto.Amount;

            var withdrawal = new Withdrawal
            {
                UserId = userId,
                Amount = dto.Amount,
                WalletAddress = dto.WalletAddress,
                Status = WithdrawalStatus.Pending
            };

            _context.Withdrawals.Add(withdrawal);

            _context.Transactions.Add(new Transaction
            {
                UserId = userId,
                Type = TransactionType.Withdrawal,
                Amount = -dto.Amount,
                Description = $"Withdrawal request to {dto.WalletAddress[..10]}..."
            });

            await _context.SaveChangesAsync();
            return withdrawal;
        }

        public async Task<List<TransactionDto>> GetTransactionsAsync(int userId, int page = 1, int pageSize = 20)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TransactionDto(
                    t.Id,
                    t.Type.ToString(),
                    t.Amount,
                    t.Description,
                    t.CreatedAt
                ))
                .ToListAsync();
        }

        public async Task<decimal> GetBalanceAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.WalletBalance ?? 0;
        }

        private string GenerateDepositAddress()
        {
            // Simulated address generation
            return "0x" + Guid.NewGuid().ToString("N")[..40];
        }
    }
}
