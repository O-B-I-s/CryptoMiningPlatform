using CryptoMining.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CryptoMining.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public AdminController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        /// <summary>
        /// Confirm a pending deposit (simulates blockchain confirmation)
        /// For testing purposes - in production, this would be automated via webhook
        /// </summary>
        [HttpPost("deposits/{depositId}/confirm")]
        public async Task<IActionResult> ConfirmDeposit(int depositId)
        {
            var result = await _walletService.ConfirmDepositAsync(depositId);

            if (!result)
                return BadRequest(new { message = "Deposit not found or already processed" });

            return Ok(new { message = "Deposit confirmed successfully" });
        }
    }
}
