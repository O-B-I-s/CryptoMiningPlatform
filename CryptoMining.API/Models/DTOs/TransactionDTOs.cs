namespace CryptoMining.API.Models.DTOs
{
    public class TransactionDTOs
    {

        public record DepositRequestDto(decimal Amount, string CryptoAddress);
        public record WithdrawalRequestDto(decimal Amount, string WalletAddress);

        public record TransactionDto(
            int Id,
            string Type,
            decimal Amount,
            string Description,
            DateTime CreatedAt
        );


    }
}
