using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Communication;
using GracelineCMS.Domain.Entities;
using GracelineCMS.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace GracelineCMS.Tests.Integration.Api
{
    public class AuthCodeApiTests
    {
#pragma warning disable CS8618
        private User _user;
#pragma warning restore CS8618

        [SetUp]
        public void Setup()
        {
            _user = GlobalFixtures.GetSavedUser();
        }
        [Test]
        public async Task CanCallApiToCreateAuthCodeAndValidateEmailSent()
        {
            var authCodeRequest = new AuthCodeRequest()
            {
                EmailAddress = _user.EmailAddress
            };
            var response = await GlobalFixtures.PostAsync($"/authentication/code", authCodeRequest);
            Assert.That(response.IsSuccessStatusCode, Is.True);
            var testEmailClient = GlobalFixtures.GetRequiredService<IEmailClient>() as TestEmailClient;
            Assert.That(testEmailClient?.SentMessages.Any(m => m.ToAddress == _user.EmailAddress), Is.True);
        }

        [Test]
        public async Task VerifyingAuthCodeWithIncorrectEmailReturnsBadRequest()
        {
            var authCodeRequest = new AuthCodeRequest()
            {
                EmailAddress = _user.EmailAddress
            };
            var response = await GlobalFixtures.PostAsync($"/authentication/code", authCodeRequest);
            Assert.That(response.IsSuccessStatusCode, Is.True);
            var authCodeValidationRequest = new AuthCodeValidationRequest()
            {
                EmailAddress = _user.EmailAddress,
                AuthCode = "randomcode"
            };
            var validationResponse = await GlobalFixtures.PostAsync($"/authentication/code/validate", authCodeValidationRequest);
            Assert.That(validationResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));
        }

        [Test]
        public async Task VerifyingAuthCodeWithCorrectEmailReturnsBearerAndAccessToken()
        {
            var email = _user.EmailAddress;
            var authCodeRequest = new AuthCodeRequest()
            {
                EmailAddress = email
            };
            var response = await GlobalFixtures.PostAsync($"/authentication/code", authCodeRequest);
            Assert.That(response.IsSuccessStatusCode, Is.True);


            var authCode = string.Empty;
            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                authCode = (await context.Users
                    .Where(m => m.EmailAddress == email.ToLower())
                    .Include(m => m.AuthCodes)
                    .FirstAsync())
                    .AuthCodes.First().Code;

            }


            var authCodeValidationRequest = new AuthCodeValidationRequest()
            {
                EmailAddress = email,
                AuthCode = authCode
            };
            var validationResponse = await GlobalFixtures.PostAsync($"/authentication/code/validate", authCodeValidationRequest);
            Assert.That(validationResponse.IsSuccessStatusCode, Is.True);


            var content = await validationResponse.Content.ReadAsStringAsync();
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            Assert.That(handler.CanReadToken(content), Is.True);

            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                var user = await context.Users
                    .Where(m => m.EmailAddress == email.ToLower())
                    .Include(m => m.RefreshTokens)
                    .FirstAsync();
                Assert.That(user.RefreshTokens.First().RefreshTokenValue, Is.EqualTo(validationResponse.GetRefreshToken()));
            }
        }

        [Test]
        public async Task VerifyingRefreshingTokenWhereRefreshTokenDoesNotExistReturnsBadRequest()
        {
            var headers = new Dictionary<string, string>
            {
                { "Cookie", $"refreshToken=badrefreshtoken" }
            };
            var response = await GlobalFixtures.GetAsync($"/token/refresh", customHeaders: headers);
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));
        }

        [Test]
        public async Task RefreshingTokenWhenTokenIsExpiredReturnsBadRequest()
        {
            var email = _user.EmailAddress;
            var authCodeRequest = new AuthCodeRequest()
            {
                EmailAddress = email
            };
            var response = await GlobalFixtures.PostAsync($"/authentication/code", authCodeRequest);
            Assert.That(response.IsSuccessStatusCode, Is.True);
            var authCode = string.Empty;

            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                authCode = (await context.Users
                    .Where(m => m.EmailAddress == email.ToLower())
                    .Include(m => m.AuthCodes)
                    .FirstAsync())
                    .AuthCodes.First().Code;
            }

            var authCodeValidationRequest = new AuthCodeValidationRequest()
            {
                EmailAddress = email,
                AuthCode = authCode
            };
            var validationResponse = await GlobalFixtures.PostAsync($"/authentication/code/validate", authCodeValidationRequest);
            Assert.That(validationResponse.IsSuccessStatusCode, Is.True);

            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                var user = await context.Users
                    .Where(m => m.EmailAddress == email.ToLower())
                    .Include(m => m.RefreshTokens)
                    .FirstAsync();
                user.RefreshTokens.First().ExpiresAt = DateTime.UtcNow.AddSeconds(-1);
                await context.SaveChangesAsync();
            }

            var headers = new Dictionary<string, string>
            {
                { "Cookie", $"refreshToken={validationResponse.GetRefreshToken()}" }
            };
            var refreshResponse = await GlobalFixtures.GetAsync($"/token/refresh", customHeaders: headers);
            Assert.That(refreshResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));
        }

        [Test]
        public async Task RefreshingTokenWhenTokenIsValidReturnsNewAccessTokenAndRefreshToken()
        {
            var email = _user.EmailAddress;
            var authCodeRequest = new AuthCodeRequest()
            {
                EmailAddress = email
            };
            var response = await GlobalFixtures.PostAsync($"/authentication/code", authCodeRequest);
            Assert.That(response.IsSuccessStatusCode, Is.True);

            var authCode = string.Empty;
            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                authCode = (await context.Users
                    .Where(m => m.EmailAddress == email.ToLower())
                    .Include(m => m.AuthCodes)
                    .FirstAsync())
                    .AuthCodes.First().Code;
            }
            var authCodeValidationRequest = new AuthCodeValidationRequest()
            {
                EmailAddress = email,
                AuthCode = authCode
            };
            var firstValidationResponse = await GlobalFixtures.PostAsync($"/authentication/code/validate", authCodeValidationRequest);

            var firstRefreshToken = firstValidationResponse.GetRefreshToken();

            var refreshResponse = await GlobalFixtures.GetAsync(
                $"/token/refresh", 
                customHeaders: new Dictionary<string, string> 
                {
                    {"Cookie", $"refreshToken={firstRefreshToken}"}
                }
            );

            var secondRefreshToken = refreshResponse.GetRefreshToken();

            Assert.That(secondRefreshToken, Is.Not.EqualTo(firstRefreshToken));

            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                var user = await context.Users
                    .Where(m => m.EmailAddress == email.ToLower())
                    .Include(m => m.RefreshTokens)
                    .FirstAsync();
                Assert.That(user.RefreshTokens.Count, Is.EqualTo(1));
                Assert.That(user.RefreshTokens.First().RefreshTokenValue, Is.EqualTo(secondRefreshToken));
            }
        }
    }
}
