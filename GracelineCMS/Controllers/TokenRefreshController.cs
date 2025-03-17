using GracelineCMS.Domain.Auth;
using GracelineCMS.Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;

namespace GracelineCMS.Controllers
{
    [Route("token/refresh")]
    [ApiController]
    public class TokenRefreshController(ITokenHandler tokenHandler) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshToken()
        {
            var httpRequest = HttpContext.Request;
            var refreshToken = httpRequest.Cookies["refreshToken"] ?? throw new Exception("No refresh token included in request");
            var accessRefreshToken = await tokenHandler.RefreshToken(refreshToken);
            Response.Cookies.Append("refreshToken", accessRefreshToken.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(14)
            });
            return Ok(accessRefreshToken.AccessToken);
        }
    }
}
