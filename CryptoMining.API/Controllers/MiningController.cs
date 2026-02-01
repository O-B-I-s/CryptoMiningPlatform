using CryptoMining.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CryptoMining.API.Models.DTOs.MiningPlanDTOs;

namespace CryptoMining.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MiningController : BaseApiController
    {
        private readonly IMiningService _miningService;

        public MiningController(IMiningService miningService)
        {
            _miningService = miningService;
        }

        [HttpGet("plans")]
        public async Task<ActionResult<List<MiningPlanDto>>> GetPlans()
        {
            var plans = await _miningService.GetAllPlansAsync();
            return Ok(plans);
        }

        [HttpGet("plans/{id}")]
        public async Task<ActionResult<MiningPlanDto>> GetPlan(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid plan ID" });

            var plan = await _miningService.GetPlanByIdAsync(id);
            if (plan == null)
                return NotFound(new { message = "Plan not found" });

            return Ok(plan);
        }

        [Authorize]
        [HttpPost("purchase")]
        public async Task<ActionResult<UserMiningPlanDto>> PurchasePlan(PurchasePlanDto dto)
        {
            if (CurrentUserId == null)
                return UnauthorizedWithMessage();

            if (dto.Amount <= 0)
                return BadRequest(new { message = "Amount must be greater than 0" });

            if (dto.MiningPlanId <= 0)
                return BadRequest(new { message = "Invalid mining plan ID" });

            var result = await _miningService.PurchasePlanAsync(CurrentUserId.Value, dto);

            if (result == null)
                return BadRequest(new { message = "Unable to purchase plan. Check balance and plan requirements." });

            return Ok(result);
        }

        [Authorize]
        [HttpGet("my-plans")]
        public async Task<ActionResult<List<UserMiningPlanDto>>> GetMyPlans()
        {
            if (CurrentUserId == null)
                return UnauthorizedWithMessage();

            var plans = await _miningService.GetUserPlansAsync(CurrentUserId.Value);
            return Ok(plans);
        }
    }
}
