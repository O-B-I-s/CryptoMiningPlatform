using static CryptoMining.API.Models.DTOs.MiningPlanDTOs;
using static CryptoMining.API.Models.DTOs.TransactionDTOs;

namespace CryptoMining.API.Models.DTOs
{
    public class DashboardDTOs
    {




        public record DashboardDto(
    decimal TotalBalance,
    decimal TotalInvested,
    decimal TotalEarnings,
    decimal TodayEarnings,
    int ActivePlans,
    List<UserMiningPlanDto> ActiveMiningPlans,
    List<TransactionDto> RecentTransactions
);

    }
}
