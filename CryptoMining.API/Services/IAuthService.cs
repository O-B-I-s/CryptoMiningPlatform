using static CryptoMining.API.Models.DTOs.AuthDTOs;
using static CryptoMining.API.Models.DTOs.UserDTOs;

namespace CryptoMining.API.Services
{
    public interface IAuthService
    {

        Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
        Task<UserProfileDto?> GetProfileAsync(int userId);
    }
}
