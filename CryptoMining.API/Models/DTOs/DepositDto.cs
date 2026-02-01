namespace CryptoMining.API.Models.DTOs
{
    public record DepositDto(
      int Id,
      decimal Amount,
      string CryptoAddress,
      string Status,
      DateTime CreatedAt,
      DateTime? ConfirmedAt
  );
}
