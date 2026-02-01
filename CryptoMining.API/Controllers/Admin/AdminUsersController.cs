using CryptoMining.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CryptoMining.API.Models.DTOs.AdminDTOs;

namespace CryptoMining.API.Controllers.Admin
{
    [Authorize]
    [ApiController]
    [Route("api/admin/users")]
    [Tags("Admin - Users")]
    public class AdminUsersController : BaseApiController
    {
        private readonly IAdminService _adminService;

        public AdminUsersController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<AdminUserDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = true)
        {
            var pagination = new PaginationParams(page, pageSize, search, sortBy, sortDescending);
            var result = await _adminService.GetUsersAsync(pagination);
            return Ok(result);
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminUserDto>> GetById(int id)
        {
            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });
            return Ok(user);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AdminUserDto>> Create(CreateUserDto dto)
        {
            try
            {
                var user = await _adminService.CreateUserAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update a user
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<AdminUserDto>> Update(int id, UpdateUserDto dto)
        {
            var user = await _adminService.UpdateUserAsync(id, dto, CurrentUsername);
            if (user == null)
                return NotFound(new { message = "User not found" });
            return Ok(user);
        }

        /// <summary>
        /// Delete (deactivate) a user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _adminService.DeleteUserAsync(id);
            if (!result)
                return NotFound(new { message = "User not found" });
            return Ok(new { message = "User deactivated successfully" });
        }

        /// <summary>
        /// Adjust user balance (credit or debit)
        /// </summary>
        [HttpPost("{id}/adjust-balance")]
        public async Task<IActionResult> AdjustBalance(int id, UserBalanceAdjustmentDto dto)
        {
            if (dto.Amount <= 0)
                return BadRequest(new { message = "Amount must be greater than 0" });

            if (string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest(new { message = "Reason is required" });

            var result = await _adminService.AdjustUserBalanceAsync(id, dto, CurrentUsername ?? "Admin");
            if (!result)
                return BadRequest(new { message = "Failed to adjust balance. User not found or insufficient funds." });

            return Ok(new { message = $"Balance {dto.Type.ToString().ToLower()}ed successfully" });
        }
    }
}
