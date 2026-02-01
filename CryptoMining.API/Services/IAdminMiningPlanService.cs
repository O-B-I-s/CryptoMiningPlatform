using static CryptoMining.API.Models.DTOs.MiningPlanAdminDTOs;

namespace CryptoMining.API.Services
{
    public interface IAdminMiningPlanService
    {
        // Read
        Task<List<MiningPlanListDto>> GetAllPlansAsync(bool includeInactive = true);
        Task<MiningPlanDetailDto?> GetPlanByIdAsync(int id);

        // Create
        Task<MiningPlanDetailDto> CreatePlanAsync(CreateMiningPlanDto dto);

        // Update
        Task<MiningPlanDetailDto?> UpdatePlanAsync(int id, UpdateMiningPlanDto dto);

        // Delete
        Task<(bool Success, string Message)> DeletePlanAsync(int id);

        // Toggle status
        Task<bool> TogglePlanStatusAsync(int id);

        // Statistics
        Task<MiningPlanStatsDto> GetPlanStatisticsAsync();
    }

    public class MiningPlanStatsDto
    {
        public int TotalPlans { get; set; }
        public int ActivePlans { get; set; }
        public int InactivePlans { get; set; }
        public int TotalActiveSubscriptions { get; set; }
        public decimal TotalInvestedAmount { get; set; }
        public decimal TotalProfitPaid { get; set; }
    }
}

