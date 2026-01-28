namespace CryptoMining.API.Models.DTOs
{
    public class UserDTOs
    {
        public record UserProfileDto(
            int Id,
            string Username,
            string Email,
            decimal WalletBalance,
            DateTime CreatedAt,
            int ActivePlans,
            decimal TotalEarnings
         );
    }
}
