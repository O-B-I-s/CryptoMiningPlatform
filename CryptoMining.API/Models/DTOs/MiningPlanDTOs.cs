namespace CryptoMining.API.Models.DTOs
{
    public class MiningPlanDTOs
    {
        public record MiningPlanDto(
            int Id,
            string Name,
            string Description,
            decimal MinDeposit,
            decimal MaxDeposit,
            decimal DailyReturnPercentage,
            int DurationDays,
            decimal HashRate
        );





        public record PurchasePlanDto(int MiningPlanId, decimal Amount);

        public record UserMiningPlanDto(
            int Id,
            string PlanName,
            decimal InvestedAmount,
            decimal TotalEarned,
            decimal DailyReturn,
            DateTime StartDate,
            DateTime EndDate,
            string Status,
            decimal HashRate
        );
    }
}
