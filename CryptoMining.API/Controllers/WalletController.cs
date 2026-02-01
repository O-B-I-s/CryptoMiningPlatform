using CryptoMining.API.Services;
using Microsoft.AspNetCore.Mvc;
using static CryptoMining.API.Models.DTOs.TransactionDTOs;

namespace CryptoMining.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : BaseApiController
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            if (CurrentUserId == null)
                return UnauthorizedWithMessage();

            var balance = await _walletService.GetBalanceAsync(CurrentUserId.Value);
            return Ok(new { balance });
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit(DepositRequestDto dto)
        {
            if (CurrentUserId == null)
                return UnauthorizedWithMessage();

            if (dto.Amount <= 0)
                return BadRequest(new { message = "Amount must be greater than 0" });

            var deposit = await _walletService.CreateDepositAsync(CurrentUserId.Value, dto);

            if (deposit == null)
                return BadRequest(new { message = "Unable to create deposit" });

            return Ok(new
            {
                depositId = deposit.Id,
                address = deposit.CryptoAddress,
                amount = deposit.Amount,
                status = deposit.Status.ToString(),
                message = "Deposit created. Send funds to the provided address."
            });
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw(WithdrawalRequestDto dto)
        {
            if (CurrentUserId == null)
                return UnauthorizedWithMessage();

            if (dto.Amount <= 0)
                return BadRequest(new { message = "Amount must be greater than 0" });

            if (string.IsNullOrWhiteSpace(dto.WalletAddress))
                return BadRequest(new { message = "Wallet address is required" });

            var withdrawal = await _walletService.RequestWithdrawalAsync(CurrentUserId.Value, dto);

            if (withdrawal == null)
                return BadRequest(new { message = "Insufficient balance" });

            return Ok(new
            {
                withdrawalId = withdrawal.Id,
                amount = withdrawal.Amount,
                walletAddress = withdrawal.WalletAddress,
                status = withdrawal.Status.ToString(),
                message = "Withdrawal request submitted"
            });
        }

        [HttpGet("transactions")]
        public async Task<ActionResult<List<TransactionDto>>> GetTransactions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (CurrentUserId == null)
                return UnauthorizedWithMessage();

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var transactions = await _walletService.GetTransactionsAsync(CurrentUserId.Value, page, pageSize);
            return Ok(transactions);
        }

        [HttpGet("deposits")]
        public async Task<IActionResult> GetDeposits([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (CurrentUserId == null)
                return UnauthorizedWithMessage();

            var deposits = await _walletService.GetDepositsAsync(CurrentUserId.Value, page, pageSize);
            return Ok(deposits);
        }

        [HttpGet("withdrawals")]
        public async Task<IActionResult> GetWithdrawals([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (CurrentUserId == null)
                return UnauthorizedWithMessage();

            var withdrawals = await _walletService.GetWithdrawalsAsync(CurrentUserId.Value, page, pageSize);
            return Ok(withdrawals);
        }
    }
}
