using GracelineCMS.Domain.Entities;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GracelineCMS.Tests.Integration.Api
{
    public class OrganizationAuthorizationApiTests
    {
#pragma warning disable CS8618
        private AuthenticatedRequestHelper _authenticatedRequestHelper;
#pragma warning restore CS8618

        [SetUp]
        public void Setup()
        {
            _authenticatedRequestHelper = GlobalFixtures.GetAuthenticatedRequestHelper();
        }

        [Test]
        public async Task CannotGetOrganizationIfUserIsNotAssociated()
        {
            var response = await GlobalFixtures.GetAsync($"/organization/{_authenticatedRequestHelper.Organization.Id}", _authenticatedRequestHelper.AuthToken);
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task CanGetOrganizationIfUserIsAssociated()
        {
            var response = await GlobalFixtures.GetAsync($"/organization/{_authenticatedRequestHelper.Organization.Id}", _authenticatedRequestHelper.AuthToken, _authenticatedRequestHelper.Headers);
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            var organizationResponse = await response.Content.ReadAsStringAsync();
            var organization = JsonConvert.DeserializeObject<Organization>(organizationResponse);
            Assert.That(organization?.Name, Is.EqualTo(_authenticatedRequestHelper.Organization.Name));
        }
    }
}
