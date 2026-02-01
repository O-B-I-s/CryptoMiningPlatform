using CryptoMining.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CryptoMining.API.Models.DTOs.AuthDTOs;
using static CryptoMining.API.Models.DTOs.UserDTOs;

namespace CryptoMining.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "All fields are required" });
            }

            var result = await _authService.RegisterAsync(dto);
            if (result == null)
                return BadRequest(new { message = "Email already registered" });

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            var result = await _authService.LoginAsync(dto);
            if (result == null)
                return Unauthorized(new { message = "Invalid credentials" });

            return Ok(result);
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            if (CurrentUserId == null)
                return UnauthorizedWithMessage();

            var profile = await _authService.GetProfileAsync(CurrentUserId.Value);

            if (profile == null)
                return NotFound(new { message = "User not found" });

            return Ok(profile);
        }
    }
}
