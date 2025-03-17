using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Entities;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace GracelineCMS.Infrastructure.Auth
{
    public class RefreshTokenHandler(IDbContextFactory<AppDbContext> dbContextFactory) : IRefreshTokenHandler
    {
        private static string GenerateRefreshToken(int size = 32)
        {
            byte[] randomBytes = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        public Task<bool> CanIssueRefreshTokenAsync(string email, string refreshToken)
        {
            return Task.FromResult(false);
        }

        public async Task<string> CreateRefreshTokenAsync(string email)
        {
            using (var context = await dbContextFactory.CreateDbContextAsync())
            {
                var user = await context.Users.Include(u => u.RefreshTokens).Where(m => m.EmailAddress == email).FirstAsync();
                var refreshToken = new RefreshToken()
                {
                    RefreshTokenValue = GenerateRefreshToken(),
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    CreatedAt = DateTime.UtcNow,
                    User = user
                };
                user.RefreshTokens.Add(refreshToken);
                await context.SaveChangesAsync();
                return refreshToken.RefreshTokenValue;
            }
        }
    }
}
