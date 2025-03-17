using System.Security.Claims;

namespace GracelineCMS.Domain.Auth
{
    public interface IClaimsProvider
    {
        List<Claim> GetClaims(string email);
    }
}
