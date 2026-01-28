using static CryptoMining.API.Models.DTOs.MiningPlanDTOs;

namespace CryptoMining.API.Services
{
    public interface IMiningService
    {
        Task<List<MiningPlanDto>> GetAllPlansAsync();
        Task<MiningPlanDto?> GetPlanByIdAsync(int id);
        Task<UserMiningPlanDto?> PurchasePlanAsync(int userId, PurchasePlanDto dto);
        Task<List<UserMiningPlanDto>> GetUserPlansAsync(int userId);
        Task CalculateProfitsAsync(); // Called by background job
    }
}
