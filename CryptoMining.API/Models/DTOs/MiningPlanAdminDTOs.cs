namespace CryptoMining.API.Models.DTOs
{
    public class MiningPlanAdminDTOs
    {


        /// <summary>
        /// DTO for creating a new mining plan
        /// </summary>
        public class CreateMiningPlanDto
        {
            /// <summary>
            /// Plan name (e.g., "Starter Plan", "Premium Plan")
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Plan description
            /// </summary>
            public string Description { get; set; } = string.Empty;

            /// <summary>
            /// Minimum deposit amount required
            /// </summary>
            public decimal MinDeposit { get; set; }

            /// <summary>
            /// Maximum deposit amount allowed
            /// </summary>
            public decimal MaxDeposit { get; set; }

            /// <summary>
            /// Return percentage per duration unit (e.g., 5 means 5%)
            /// </summary>
            public decimal ReturnPercentage { get; set; }

            /// <summary>
            /// Duration value (e.g., 30 for "30 minutes" or "30 days")
            /// </summary>
            public int DurationValue { get; set; }

            /// <summary>
            /// Duration unit: Minutes, Hours, Days, Weeks, Months
            /// </summary>
            public string DurationUnit { get; set; } = "Days";

            /// <summary>
            /// Simulated hash rate in TH/s
            /// </summary>
            public decimal HashRate { get; set; }

            /// <summary>
            /// Whether the plan is active and available for purchase
            /// </summary>
            public bool IsActive { get; set; } = true;
        }

        /// <summary>
        /// DTO for updating an existing mining plan
        /// </summary>
        public class UpdateMiningPlanDto
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
            public decimal? MinDeposit { get; set; }
            public decimal? MaxDeposit { get; set; }
            public decimal? ReturnPercentage { get; set; }
            public int? DurationValue { get; set; }
            public string? DurationUnit { get; set; }
            public decimal? HashRate { get; set; }
            public bool? IsActive { get; set; }
        }

        /// <summary>
        /// Detailed mining plan response for admin
        /// </summary>
        public class MiningPlanDetailDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal MinDeposit { get; set; }
            public decimal MaxDeposit { get; set; }
            public decimal ReturnPercentage { get; set; }
            public int DurationValue { get; set; }
            public string DurationUnit { get; set; } = string.Empty;
            public decimal HashRate { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public int TotalSubscribers { get; set; }
            public int ActiveSubscribers { get; set; }
            public decimal TotalInvested { get; set; }
            public decimal TotalProfitPaid { get; set; }
        }

        /// <summary>
        /// Simple mining plan list item
        /// </summary>
        public class MiningPlanListDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public decimal MinDeposit { get; set; }
            public decimal MaxDeposit { get; set; }
            public decimal ReturnPercentage { get; set; }
            public int DurationValue { get; set; }
            public string DurationUnit { get; set; } = string.Empty;
            public decimal HashRate { get; set; }
            public bool IsActive { get; set; }
            public int ActiveSubscribers { get; set; }
        }
    }
}
