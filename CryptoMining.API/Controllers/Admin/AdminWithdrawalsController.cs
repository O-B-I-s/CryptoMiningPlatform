using CryptoMining.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CryptoMining.API.Models.DTOs.AdminDTOs;

namespace CryptoMining.API.Controllers.Admin
{
    [Authorize]
    [ApiController]
    [Route("api/admin/withdrawals")]
    [Tags("Admin - Withdrawals")]
    public class AdminWithdrawalsController : BaseApiController
    {
        private readonly IAdminService _adminService;

        public AdminWithdrawalsController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>
        /// Get all withdrawals with pagination
        /// </summary>
        /// <param name="status">Filter by status: Pending, Processing, Completed, Rejected</param>
        [HttpGet]
        public async Task<ActionResult<PagedResult<AdminWithdrawalDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null)
        {
            var pagination = new PaginationParams(page, pageSize, search);
            var result = await _adminService.GetWithdrawalsAsync(pagination, status);
            return Ok(result);
        }

        /// <summary>
        /// Get withdrawal by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminWithdrawalDto>> GetById(int id)
        {
            var withdrawal = await _adminService.GetWithdrawalByIdAsync(id);
            if (withdrawal == null)
                return NotFound(new { message = "Withdrawal not found" });
            return Ok(withdrawal);
        }

        /// <summary>
        /// Update a withdrawal
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<AdminWithdrawalDto>> Update(int id, UpdateWithdrawalDto dto)
        {
            var withdrawal = await _adminService.UpdateWithdrawalAsync(id, dto, CurrentUsername ?? "Admin");
            if (withdrawal == null)
                return NotFound(new { message = "Withdrawal not found" });
            return Ok(withdrawal);
        }

        /// <summary>
        /// Approve a pending withdrawal
        /// </summary>
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _adminService.ApproveWithdrawalAsync(id, CurrentUsername ?? "Admin");
            if (!result) return BadRequest(new { message = "Withdrawal not found or not pending" });
            return Ok(new { message = "Withdrawal approved successfully" });
        }

        /// <summary>
        /// Reject a pending withdrawal (refunds the user)
        /// </summary>
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest(new { message = "Rejection reason is required" });

            var result = await _adminService.RejectWithdrawalAsync(id, request.Reason, CurrentUsername ?? "Admin");
            if (!result)
                return BadRequest(new { message = "Withdrawal not found or not pending" });
            return Ok(new { message = "Withdrawal rejected and funds refunded" });
        }

        /// <summary>
        /// Delete a withdrawal
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _adminService.DeleteWithdrawalAsync(id);
            if (!result)
                return NotFound(new { message = "Withdrawal not found" });
            return Ok(new { message = "Withdrawal deleted successfully" });
        }
    }
}
