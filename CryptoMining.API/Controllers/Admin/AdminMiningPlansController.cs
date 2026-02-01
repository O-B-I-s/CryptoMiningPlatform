using CryptoMining.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CryptoMining.API.Models.DTOs.MiningPlanAdminDTOs;

namespace CryptoMining.API.Controllers.Admin
{
    [Authorize]
    [ApiController]
    [Route("api/admin/mining-plans")]
    [Tags("Admin - Mining Plans")]
    public class AdminMiningPlansController : ControllerBase
    {
        private readonly IAdminMiningPlanService _miningPlanService;
        private readonly ILogger<AdminMiningPlansController> _logger;

        public AdminMiningPlansController(
            IAdminMiningPlanService miningPlanService,
            ILogger<AdminMiningPlansController> logger)
        {
            _miningPlanService = miningPlanService;
            _logger = logger;
        }

        /// <summary>
        /// Get all mining plans
        /// </summary>
        /// <param name="includeInactive">Include inactive plans (default: true)</param>
        [HttpGet]
        [ProducesResponseType(typeof(List<MiningPlanListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<MiningPlanListDto>>> GetAll([FromQuery] bool includeInactive = true)
        {
            var plans = await _miningPlanService.GetAllPlansAsync(includeInactive);
            return Ok(plans);
        }

        /// <summary>
        /// Get mining plan by ID with full details
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MiningPlanDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MiningPlanDetailDto>> GetById(int id)
        {
            var plan = await _miningPlanService.GetPlanByIdAsync(id);

            if (plan == null)
                return NotFound(new { message = "Mining plan not found" });

            return Ok(plan);
        }

        /// <summary>
        /// Create a new mining plan
        /// </summary>
        /// <remarks>
        /// Duration units available:
        /// - **Minutes**: Profit calculated every X minutes
        /// - **Hours**: Profit calculated every X hours
        /// - **Days**: Profit calculated every X days
        /// - **Weeks**: Profit calculated every X weeks
        /// - **Months**: Profit calculated every X months
        /// 
        /// Example configurations:
        /// - 1% every 5 minutes: `returnPercentage: 1, durationValue: 5, durationUnit: "Minutes"`
        /// - 5% daily: `returnPercentage: 5, durationValue: 1, durationUnit: "Days"`
        /// - 20% weekly: `returnPercentage: 20, durationValue: 1, durationUnit: "Weeks"`
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(MiningPlanDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MiningPlanDetailDto>> Create([FromBody] CreateMiningPlanDto dto)
        {
            try
            {
                var plan = await _miningPlanService.CreatePlanAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update a mining plan
        /// </summary>
        /// <remarks>
        /// Only provide fields you want to update. Null/missing fields will not be changed.
        /// </remarks>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MiningPlanDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MiningPlanDetailDto>> Update(int id, [FromBody] UpdateMiningPlanDto dto)
        {
            try
            {
                var plan = await _miningPlanService.UpdatePlanAsync(id, dto);

                if (plan == null)
                    return NotFound(new { message = "Mining plan not found" });

                return Ok(plan);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a mining plan
        /// </summary>
        /// <remarks>
        /// - Plans with active subscriptions will be deactivated instead of deleted
        /// - Plans with historical subscriptions will be deactivated instead of deleted
        /// - Plans with no subscriptions will be permanently deleted
        /// </remarks>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _miningPlanService.DeletePlanAsync(id);

            if (!success)
                return NotFound(new { message });

            return Ok(new { message });
        }

        /// <summary>
        /// Toggle mining plan active/inactive status
        /// </summary>
        [HttpPost("{id}/toggle-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var success = await _miningPlanService.TogglePlanStatusAsync(id);

            if (!success)
                return NotFound(new { message = "Mining plan not found" });

            return Ok(new { message = "Plan status toggled successfully" });
        }

        /// <summary>
        /// Get mining plan statistics
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(MiningPlanStatsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<MiningPlanStatsDto>> GetStatistics()
        {
            var stats = await _miningPlanService.GetPlanStatisticsAsync();
            return Ok(stats);
        }
    }
}
