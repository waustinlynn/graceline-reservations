using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Entities;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace GracelineCMS.Infrastructure.Auth
{
    public class AuthenticationCode(
        IDbContextFactory<AppDbContext> dbContextFactory
    ) : IAuthenticationCode
    {
        private static int RandomCode()
        {
            return Random.Shared.Next(100000, 999999);
        }

        public async Task<string> CreateAuthCodeAsync(string email)
        {
            email = email.ToLower();
            using (var context = await dbContextFactory.CreateDbContextAsync())
            {
                var user = await context.Users.Include(u => u.AuthCodes).FirstOrDefaultAsync(u => u.EmailAddress == email);
                if (user == null)
                {
                    throw new Exception("No user found with that email address.");
                }
                var randomizedCode = RandomCode().ToString();
                if (user.AuthCodes.Count > 0)
                {
                    var firstAuthCode = user.AuthCodes.First();
                    firstAuthCode.Code = randomizedCode;
                    firstAuthCode.ExpiresAt = DateTime.UtcNow.AddMinutes(10);
                }
                else
                {
                    var authCode = new AuthCode()
                    {
                        Code = randomizedCode,
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                        User = user
                    };
                    user.AuthCodes.Add(authCode);
                }
                await context.SaveChangesAsync();
                return randomizedCode;
            }
        }

        public async Task DeleteExpiredCodes()
        {
            using (var context = await dbContextFactory.CreateDbContextAsync())
            {
                var expiredCodes = await context.AuthCodes
                    .Where(m => m.ExpiresAt < DateTime.UtcNow)
                    .ToListAsync();
                context.AuthCodes.RemoveRange(expiredCodes);
                await context.SaveChangesAsync();
            }
        }

        public async Task ValidateCodeWithEmail(string email, string code)
        {
            using (var context = await dbContextFactory.CreateDbContextAsync())
            {
                var authCode = context.Users
                    .Include(u => u.AuthCodes)
                    .First(u => u.EmailAddress == email.ToLower())
                    .AuthCodes
                    .FirstOrDefault(m => m.Code == code);
                if (authCode == null)
                {
                    throw new Exception("Missing auth code in database");
                }
                if (authCode.ExpiresAt < DateTime.UtcNow)
                {
                    throw new Exception("Auth code has expired");
                }
            }
        }
    }
}
