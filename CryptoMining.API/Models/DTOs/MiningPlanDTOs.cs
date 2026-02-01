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
     decimal ReturnPercentage,
     int DurationValue,
     string DurationUnit,
     decimal HashRate
 );

        public record PurchasePlanDto(int MiningPlanId, decimal Amount);

        public record UserMiningPlanDto(
            int Id,
            string PlanName,
            decimal InvestedAmount,
            decimal TotalEarned,
            decimal ExpectedReturn,
            DateTime StartDate,
            DateTime EndDate,
            string Status,
            decimal HashRate,
            int DurationValue,
            string DurationUnit
        );
    }
}
