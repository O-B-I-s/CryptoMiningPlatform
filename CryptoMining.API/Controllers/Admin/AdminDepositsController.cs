using CryptoMining.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CryptoMining.API.Models.DTOs.AdminDTOs;

namespace CryptoMining.API.Controllers.Admin
{
    [Authorize]
    [ApiController]
    [Route("api/admin/deposits")]
    [Tags("Admin - Deposits")]
    public class AdminDepositsController : BaseApiController
    {
        private readonly IAdminService _adminService;

        public AdminDepositsController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>
        /// Get all deposits with pagination
        /// </summary>
        /// <param name="status">Filter by status: Pending, Confirmed, Failed, Cancelled</param>
        [HttpGet]
        public async Task<ActionResult<PagedResult<AdminDepositDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null)
        {
            var pagination = new PaginationParams(page, pageSize, search);
            var result = await _adminService.GetDepositsAsync(pagination, status);
            return Ok(result);
        }

        /// <summary>
        /// Get deposit by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminDepositDto>> GetById(int id)
        {
            var deposit = await _adminService.GetDepositByIdAsync(id);
            if (deposit == null)
                return NotFound(new { message = "Deposit not found" });
            return Ok(deposit);
        }

        /// <summary>
        /// Create a deposit for a user (admin can directly add funds)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AdminDepositDto>> Create(CreateDepositDto dto)
        {
            try
            {
                var deposit = await _adminService.CreateDepositAsync(dto, CurrentUsername ?? "Admin");
                return CreatedAtAction(nameof(GetById), new { id = deposit.Id }, deposit);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update a deposit
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<AdminDepositDto>> Update(int id, UpdateDepositDto dto)
        {
            var deposit = await _adminService.UpdateDepositAsync(id, dto, CurrentUsername ?? "Admin");
            if (deposit == null)
                return NotFound(new { message = "Deposit not found" });
            return Ok(deposit);
        }

        /// <summary>
        /// Confirm a pending deposit
        /// </summary>
        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            var result = await _adminService.ConfirmDepositAsync(id, CurrentUsername ?? "Admin");
            if (!result)
                return BadRequest(new { message = "Deposit not found or not pending" });
            return Ok(new { message = "Deposit confirmed successfully" });
        }

        /// <summary>
        /// Reject a pending deposit
        /// </summary>
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest(new { message = "Rejection reason is required" });

            var result = await _adminService.RejectDepositAsync(id, request.Reason, CurrentUsername ?? "Admin");
            if (!result)
                return BadRequest(new { message = "Deposit not found or not pending" });
            return Ok(new { message = "Deposit rejected successfully" });
        }

        /// <summary>
        /// Delete a deposit (only pending/failed)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _adminService.DeleteDepositAsync(id);
            if (!result)
                return BadRequest(new { message = "Deposit not found or cannot be deleted (already confirmed)" });
            return Ok(new { message = "Deposit deleted successfully" });
        }
    }

    public record RejectRequestDto(string Reason);
}
