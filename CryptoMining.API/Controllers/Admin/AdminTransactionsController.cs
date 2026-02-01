using CryptoMining.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CryptoMining.API.Models.DTOs.AdminDTOs;

namespace CryptoMining.API.Controllers.Admin
{
    [Authorize]
    [ApiController]
    [Route("api/admin/transactions")]
    [Tags("Admin - Transactions")]
    public class AdminTransactionsController : BaseApiController
    {
        private readonly IAdminService _adminService;

        public AdminTransactionsController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>
        /// Get all transactions with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<AdminTransactionDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] int? userId = null)
        {
            var pagination = new PaginationParams(page, pageSize, search);
            var result = await _adminService.GetTransactionsAsync(pagination, userId);
            return Ok(result);
        }

        /// <summary>
        /// Get transactions for a specific user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<PagedResult<AdminTransactionDto>>> GetByUser(
            int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var pagination = new PaginationParams(page, pageSize);
            var result = await _adminService.GetTransactionsAsync(pagination, userId);
            return Ok(result);
        }
    }
}
