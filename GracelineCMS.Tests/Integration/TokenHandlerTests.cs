using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;

namespace GracelineCMS.Tests.Integration
{
    public class TokenHandlerTests
    {

#pragma warning disable CS8618
        private ITokenHandler _tokenHandler;
        private User _user;
#pragma warning restore CS8618

        [SetUp]
        public void Setup()
        {
            _user = GlobalFixtures.GetSavedUser();
            _tokenHandler = GlobalFixtures.GetRequiredService<ITokenHandler>();
        }

        [Test]
        public async Task CanCreateTokenAndRefreshTokenFromEmail()
        {
            AccessRefreshToken accessRefreshToken = await _tokenHandler.CreateAccessAndRefreshToken(_user.EmailAddress);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            Assert.That(handler.CanReadToken(accessRefreshToken.AccessToken), Is.True);
        }

        [Test]
        public void RefreshingMissingTokenThrowsException()
        {
            Assert.ThrowsAsync<Exception>(async () => await _tokenHandler.RefreshToken("missing token"));
        }

        [Test]
        public async Task RefreshingExpiredTokenThrowsException()
        {
            AccessRefreshToken accessRefreshToken = await _tokenHandler.CreateAccessAndRefreshToken(_user.EmailAddress);
            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                var user = context.Users.Include(u => u.RefreshTokens).First(u => u.EmailAddress == _user.EmailAddress);
                var refreshToken = user.RefreshTokens.First();
                refreshToken.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
                await context.SaveChangesAsync();
            }
            Assert.ThrowsAsync<Exception>(async () => await _tokenHandler.RefreshToken(accessRefreshToken.RefreshToken));
        }

        [Test]
        public async Task CanRefreshToken()
        {
            AccessRefreshToken accessRefreshToken = await _tokenHandler.CreateAccessAndRefreshToken(_user.EmailAddress);
            AccessRefreshToken newAccessRefreshToken = await _tokenHandler.RefreshToken(accessRefreshToken.RefreshToken);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            Assert.That(handler.CanReadToken(newAccessRefreshToken.AccessToken), Is.True);
            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                var user = await context.Users.Include(u => u.RefreshTokens).FirstAsync(u => u.EmailAddress == _user.EmailAddress);
                Assert.That(user.RefreshTokens.Count, Is.EqualTo(1));
                Assert.That(user.RefreshTokens.First().RefreshTokenValue, Is.Not.EqualTo(accessRefreshToken.RefreshToken));
                Assert.That(user.RefreshTokens.First().RefreshTokenValue, Is.EqualTo(newAccessRefreshToken.RefreshToken));
            }
        }
    }
}
