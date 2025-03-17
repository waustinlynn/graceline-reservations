using GracelineCMS.Domain.Auth;
using GracelineCMS.Infrastructure.Auth;
using GracelineCMS.Infrastructure.Organization;
using GracelineCMS.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Net;

namespace GracelineCMS.Tests.Integration.Api
{
    public class OrganizationApiTests
    {
        [Test]
        public async Task CannotCreateOrganizationIfNotAGlobalAdmin()
        {
            var user = GlobalFixtures.GetSavedUser();
            var organizationRequest = new CreateOrganizationRequest()
            {
                Name = "Test Organization"
            };
            var authToken = GlobalFixtures.GetAuthToken(user.EmailAddress);
            var response = await GlobalFixtures.PostAsync($"/organization", organizationRequest, authToken);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task CanCreateOrganizationIfGlobalAdmin()
        {
            var fakeOrganization = FakeOrganization.Organization;
            var organizationRequest = new CreateOrganizationRequest()
            {
                Name = fakeOrganization.Name
            };
            var authToken = GlobalFixtures.GetAuthToken(GlobalFixtures.GlobalAdminEmail);
            var response = await GlobalFixtures.PostAsync($"/organization", organizationRequest, authToken);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                var organization = await context.Organizations.FirstOrDefaultAsync(m => m.Name == fakeOrganization.Name);
                Assert.That(organization, Is.Not.Null);
            }
        }

        [Test]
        public async Task CannotCreateOrganizationIfTokenSignedWithDifferentSecret()
        {
            var claimsProvider = GlobalFixtures.GetRequiredService<IClaimsProvider>();
            var badSecret = "9238hj9028hg092yht9g028y40t9283y940t82h789gyh274ty040ygft4801y74t90ty40789";
            ITokenHandler tokenHandler = new AppTokenHandler(claimsProvider, badSecret, GlobalFixtures.DbContextFactory);
            var accessRefreshToken = await tokenHandler.CreateAccessAndRefreshToken(GlobalFixtures.GlobalAdminEmail);

            var fakeOrganization = FakeOrganization.Organization;
            var organizationRequest = new CreateOrganizationRequest()
            {
                Name = fakeOrganization.Name
            };
            var response = await GlobalFixtures.PostAsync($"/organization", organizationRequest, accessRefreshToken.AccessToken);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }
}
