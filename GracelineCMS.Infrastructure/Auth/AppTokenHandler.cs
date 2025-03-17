using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Entities;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GracelineCMS.Infrastructure.Auth
{
    public class AppTokenHandler(IClaimsProvider claimsProvider, string secret, IDbContextFactory<AppDbContext> dbContextFactory) : ITokenHandler
    {
        public async Task<AccessRefreshToken> CreateAccessAndRefreshToken(string email)
        {
            using (var context = await dbContextFactory.CreateDbContextAsync())
            {
                var user = await context.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.EmailAddress == email);
                if (user == null)
                {
                    throw new Exception("No user found with that email address.");
                }
                var accessToken = CreateToken(email);
                var refreshToken = GenerateRefreshToken();
                var existingTokens = user.RefreshTokens;
                context.RefreshTokens.RemoveRange(existingTokens);
                user.RefreshTokens = new List<RefreshToken>()
                {
                    new RefreshToken
                    {
                        RefreshTokenValue = refreshToken,
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddDays(14),
                        User = user
                    }
                };
                await context.SaveChangesAsync();
                return new AccessRefreshToken
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
            }
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            RandomNumberGenerator.Fill(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string CreateToken(string email)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Email, email),
            };
            claims.AddRange(claimsProvider.GetClaims(email));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<AccessRefreshToken> RefreshToken(string refreshToken)
        {
            using (var context = await dbContextFactory.CreateDbContextAsync())
            {
                var user = await context.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.RefreshTokenValue == refreshToken));
                if (user == null)
                {
                    throw new Exception("Missing user by email to refresh token");
                }
                var token = user.RefreshTokens.FirstOrDefault(t => t.RefreshTokenValue == refreshToken);
                if (token == null)
                {
                    throw new Exception("Missing refresh token");
                }
                if (token.ExpiresAt < DateTime.UtcNow)
                {
                    throw new Exception("Refresh token is expired");
                }
                return await CreateAccessAndRefreshToken(user.EmailAddress);
            }
        }
    }
}
