using GracelineCMS.Domain.Auth;
using System.Security.Claims;

namespace GracelineCMS.Infrastructure.Auth
{
    public class ClaimsProvider(string globalAdminEmail) : IClaimsProvider
    {
        public List<Claim> GetClaims(string email)
        {
            if (email == globalAdminEmail)
            {
                return new List<Claim>
                {
                    new Claim(ClaimTypes.Role, "GlobalAdmin")
                };
            }
            return new List<Claim>();
        }
    }
}
