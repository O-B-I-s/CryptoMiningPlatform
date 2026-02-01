namespace CryptoMining.API.Models.DTOs
{
    public record WithdrawalDto(
     int Id,
     decimal Amount,
     string WalletAddress,
     string Status,
     DateTime CreatedAt,
     DateTime? ProcessedAt
 );
}
