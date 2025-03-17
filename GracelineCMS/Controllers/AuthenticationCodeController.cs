using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Communication;
using GracelineCMS.Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;

namespace GracelineCMS.Controllers
{
    [Route("authentication/code")]
    [ApiController]
    public class AuthenticationCodeController(
        IAuthenticationCodeEmail authenticationCodeEmail,
        IAuthenticationCode authenticationCode,
        ITokenHandler tokenHandler
    ) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateAuthCode([FromBody] AuthCodeRequest request)
        {
            await authenticationCodeEmail.GetCodeAndEmailUser(request.EmailAddress);
            return Created();
        }

        [HttpPost("validate")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateAuthCode([FromBody] AuthCodeValidationRequest request)
        {
            await authenticationCode.ValidateCodeWithEmail(request.EmailAddress, request.AuthCode);
            var accessRefreshToken = await tokenHandler.CreateAccessAndRefreshToken(request.EmailAddress);
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
