using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CryptoMining.API.Controllers
{

    [ApiController]
    public class BaseApiController : ControllerBase
    {
        protected int? CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim))
                    return null;

                if (int.TryParse(userIdClaim, out int userId))
                    return userId;

                return null;
            }
        }

        protected string? CurrentUserEmail => User.FindFirstValue(ClaimTypes.Email);
        protected string? CurrentUsername => User.FindFirstValue(ClaimTypes.Name);

        protected ActionResult UnauthorizedWithMessage(string message = "Invalid token. Please login again.")
        {
            return Unauthorized(new { message });
        }
    }
}
