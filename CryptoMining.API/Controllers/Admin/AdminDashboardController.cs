using CryptoMining.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CryptoMining.API.Models.DTOs.AdminDTOs;

namespace CryptoMining.API.Controllers.Admin
{
    [Authorize]
    [ApiController]
    [Route("api/admin/dashboard")]
    [Tags("Admin - Dashboard")]
    public class AdminDashboardController : BaseApiController
    {
        private readonly IAdminService _adminService;

        public AdminDashboardController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>
        /// Get admin dashboard statistics
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<AdminDashboardDto>> GetDashboard()
        {
            var dashboard = await _adminService.GetDashboardAsync();
            return Ok(dashboard);
        }
    }
}
