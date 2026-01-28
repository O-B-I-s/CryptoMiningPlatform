using CryptoMining.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static CryptoMining.API.Models.DTOs.MiningPlanDTOs;

namespace CryptoMining.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MiningController : ControllerBase
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
            var plan = await _miningService.GetPlanByIdAsync(id);
            if (plan == null)
                return NotFound();

            return Ok(plan);
        }

        [Authorize]
        [HttpPost("purchase")]
        public async Task<ActionResult<UserMiningPlanDto>> PurchasePlan(PurchasePlanDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _miningService.PurchasePlanAsync(userId, dto);

            if (result == null)
                return BadRequest(new { message = "Unable to purchase plan. Check balance and plan requirements." });

            return Ok(result);
        }

        [Authorize]
        [HttpGet("my-plans")]
        public async Task<ActionResult<List<UserMiningPlanDto>>> GetMyPlans()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var plans = await _miningService.GetUserPlansAsync(userId);
            return Ok(plans);
        }
    }
}
