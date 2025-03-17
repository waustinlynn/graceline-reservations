using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Entities;
using GracelineCMS.Infrastructure.Auth;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GracelineCMS.Tests.Integration
{
    public class RefreshTokenHandlerTests
    {
#pragma warning disable CS8618
        private IDbContextFactory<AppDbContext> _dbContextFactory;
        private IRefreshTokenHandler _refreshTokenHandler;
        private User _user;
#pragma warning restore CS8618

        [SetUp]
        public void Setup()
        {
            _dbContextFactory = GlobalFixtures.DbContextFactory;
            _refreshTokenHandler = new RefreshTokenHandler(_dbContextFactory);
            _user = GlobalFixtures.GetSavedUser();
        }
        [Test]
        public async Task RefreshingTokenWhenMissingInDatabaseThrowsException()
        {
            var refreshToken = "somerefreshtoken";
            bool canIssueRefreshToken = await _refreshTokenHandler.CanIssueRefreshTokenAsync(_user.EmailAddress, refreshToken);
            Assert.That(canIssueRefreshToken, Is.False);
        }

        [Test]
        public async Task CanCreateRefreshToken()
        {
            string refreshToken = await _refreshTokenHandler.CreateRefreshTokenAsync(_user.EmailAddress);
            Assert.That(refreshToken, Is.Not.Null);
            using (var context = await _dbContextFactory.CreateDbContextAsync())
            {
                var user = await context.Users.Include(u => u.RefreshTokens).Where(m => m.EmailAddress == _user.EmailAddress).FirstAsync();
                Assert.That(user.RefreshTokens.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task RefreshingTokenWhenRefreshTokenIsExpiredReturnsFalse()
        {
            var email = _user.EmailAddress;
            var refreshToken = await _refreshTokenHandler.CreateRefreshTokenAsync(email);
            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                var user = await context.Users.Include(u => u.RefreshTokens).Where(m => m.EmailAddress == email).FirstAsync();
                var savedRefreshToken = user.RefreshTokens.First();
                savedRefreshToken.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
                await context.SaveChangesAsync();
            }
            var canRefreshToken = await _refreshTokenHandler.CanIssueRefreshTokenAsync(email, refreshToken);
            Assert.That(canRefreshToken, Is.False);
        }

        [Test]
        public void CreateRefreshTokenForMissingUserThrowsException()
        {
            Assert.Throws<AggregateException>(() =>
            {
                _refreshTokenHandler.CreateRefreshTokenAsync("missinguser").Wait();
            });
        }
    }
}
