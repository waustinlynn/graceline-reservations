using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Entities;
using GracelineCMS.Infrastructure.Auth;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GracelineCMS.Tests.Integration
{
    public class AuthenticationCodeTests
    {
#pragma warning disable CS8618
        IAuthenticationCode _authenticationCode;
        IDbContextFactory<AppDbContext> _dbContextFactory;
        User _user;
#pragma warning restore CS8618

        [SetUp]
        public void Setup()
        {
            _dbContextFactory = GlobalFixtures.DbContextFactory;
            _authenticationCode = new AuthenticationCode(_dbContextFactory);
            _user = GlobalFixtures.GetSavedUser();
        }
        [Test]
        public async Task CanCreateAuthCode()
        {
            await _authenticationCode.CreateAuthCodeAsync(_user.EmailAddress);

            using (var context = await _dbContextFactory.CreateDbContextAsync())
            {
                var authCode = (
                    await context.Users.Include(m => m.AuthCodes).Where(m => m.EmailAddress == _user.EmailAddress).FirstAsync()
                ).AuthCodes.First().Code;
                Assert.That(authCode, Is.Not.Null);
            }
        }

        [Test]
        public async Task CanDeleteAuthCodesThatAreExpired()
        {
            await _authenticationCode.CreateAuthCodeAsync(_user.EmailAddress);
            using (var context = await _dbContextFactory.CreateDbContextAsync())
            {
                var authCode = (await context.Users.Include(u => u.AuthCodes).Where(u => u.EmailAddress == _user.EmailAddress).FirstAsync()).AuthCodes.First();
                authCode.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
                await context.SaveChangesAsync();

            }
            await _authenticationCode.DeleteExpiredCodes();

            using (var context = await _dbContextFactory.CreateDbContextAsync())
            {
                var authCodes = await context.AuthCodes.Where(a => a.User.EmailAddress == _user.EmailAddress).CountAsync();
                Assert.That(authCodes, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task ValidateBadCodeWithEmailThrowsException()
        {
            await _authenticationCode.CreateAuthCodeAsync(_user.EmailAddress);
            Assert.ThrowsAsync<Exception>(async () => await _authenticationCode.ValidateCodeWithEmail(_user.EmailAddress, "badcode"));
        }

        [Test]
        public async Task ValidateCodeWithEmailReturnsTrue()
        {
            var code = await _authenticationCode.CreateAuthCodeAsync(_user.EmailAddress);
            await _authenticationCode.ValidateCodeWithEmail(_user.EmailAddress, code);
            Assert.That(true, Is.True); //no exception thrown indicates valid validation
        }

        [Test]
        public async Task ValidateCodeGenerationIsUnique()
        {
            var user2 = new User
            {
                EmailAddress = "test2@email.com"
            };
            using (var context = _dbContextFactory.CreateDbContext())
            {
                context.Users.Add(user2);
                context.SaveChanges();
            }
            var code1 = await _authenticationCode.CreateAuthCodeAsync(_user.EmailAddress);
            var code2 = await _authenticationCode.CreateAuthCodeAsync(user2.EmailAddress);
            Assert.That(code1, Is.Not.EqualTo(code2));
        }

        [Test]
        public void CreatingAuthCodeForMissingUserThrowsException()
        {
            Assert.Throws<AggregateException>(() =>
            {
                _authenticationCode.CreateAuthCodeAsync("missinguseremail").Wait();
            });
        }

        [Test]
        public async Task CanCreateAuthCodeAndUserForGlobalAdmin()
        {
            var globalAdminEmail = GlobalFixtures.GlobalAdminEmail;
            var code = await _authenticationCode.CreateAuthCodeAsync(globalAdminEmail);
            Assert.That(code, Is.Not.Null);
        }
    }
}
