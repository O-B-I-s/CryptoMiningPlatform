using CryptoMining.API.Data;
using CryptoMining.API.Models.DTOs;
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
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return null;

            var deposit = new Deposit
            {
                UserId = userId,
                Amount = dto.Amount,
                CryptoAddress = GenerateDepositAddress(),
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
                Description = $"Deposit confirmed - ${deposit.Amount:F2} credited"
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Withdrawal?> RequestWithdrawalAsync(int userId, WithdrawalRequestDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return null;

            // Check sufficient balance
            if (user.WalletBalance < dto.Amount)
                return null;

            // Minimum withdrawal check (optional)
            if (dto.Amount < 10)
                return null;

            // Deduct from wallet (hold funds)
            user.WalletBalance -= dto.Amount;

            var withdrawal = new Withdrawal
            {
                UserId = userId,
                Amount = dto.Amount,
                WalletAddress = dto.WalletAddress,
                Status = WithdrawalStatus.Pending
            };

            _context.Withdrawals.Add(withdrawal);

            // Create transaction record with safe address shortening
            var shortAddress = ShortenAddress(dto.WalletAddress);
            _context.Transactions.Add(new Transaction
            {
                UserId = userId,
                Type = TransactionType.Withdrawal,
                Amount = -dto.Amount,
                Description = $"Withdrawal to {shortAddress}"
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

        public async Task<List<DepositDto>> GetDepositsAsync(int userId, int page = 1, int pageSize = 20)
        {
            return await _context.Deposits
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DepositDto(
                    d.Id,
                    d.Amount,
                    d.CryptoAddress,
                    d.Status.ToString(),
                    d.CreatedAt,
                    d.ConfirmedAt
                ))
                .ToListAsync();
        }

        public async Task<List<WithdrawalDto>> GetWithdrawalsAsync(int userId, int page = 1, int pageSize = 20)
        {
            return await _context.Withdrawals
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new WithdrawalDto(
                    w.Id,
                    w.Amount,
                    w.WalletAddress,
                    w.Status.ToString(),
                    w.CreatedAt,
                    w.ProcessedAt
                ))
                .ToListAsync();
        }

        // ============================================
        // HELPER METHODS - FIXED
        // ============================================

        /// <summary>
        /// Generate a simulated Ethereum-style deposit address
        /// </summary>
        private static string GenerateDepositAddress()
        {
            var bytes = new byte[20]; // 20 bytes = 40 hex characters
            Random.Shared.NextBytes(bytes);
            return "0x" + Convert.ToHexString(bytes).ToLowerInvariant();
        }

        /// <summary>
        /// Safely shorten a wallet address for display
        /// </summary>
        private static string ShortenAddress(string? address)
        {
            // Handle null or empty
            if (string.IsNullOrWhiteSpace(address))
                return "Unknown";

            // Trim any whitespace
            address = address.Trim();

            // If address is too short to shorten meaningfully, return as-is
            if (address.Length <= 14)
                return address;

            // Return first 6 + ... + last 4 characters
            // Example: "0x1234567890abcdef1234567890abcdef12345678" -> "0x1234...5678"
            return string.Concat(address.AsSpan(0, 6), "...", address.AsSpan(address.Length - 4));
        }
    }
}
