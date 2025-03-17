using GracelineCMS.Domain.Entities;
using GracelineCMS.Infrastructure.Content;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Net;

namespace GracelineCMS.Tests.Integration.Api
{
    public class ContentModuleApiTests
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
        public async Task CannotCreateContentModuleIfMissingOrganization()
        {
            var organizationId = Guid.NewGuid().ToString();
            var createContentModuleRequest = new ContentModuleRequest()
            {
                OrganizationId = organizationId,
                Name = "Test Content Module",
                Description = "This is a test content module"
            };
            var response = await GlobalFixtures.PostAsync("/contentmodule", createContentModuleRequest, _authenticatedRequestHelper.AuthToken, _authenticatedRequestHelper.Headers);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task CanCreateContentModule()
        {
            var createContentModuleRequest = new ContentModuleRequest()
            {
                OrganizationId = _authenticatedRequestHelper.Organization.Id,
                Name = "Test Content Module",
                Description = "This is a test content module"
            };
            var response = await GlobalFixtures.PostAsync("/contentmodule", createContentModuleRequest, _authenticatedRequestHelper.AuthToken, _authenticatedRequestHelper.Headers);
            Assert.That(response.IsSuccessStatusCode);
            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                var contentModule = await context.ContentModules.Include(c => c.Organization).FirstOrDefaultAsync(m => m.Name == createContentModuleRequest.Name);
                Assert.That(contentModule, Is.Not.Null);
                Assert.That(contentModule?.Organization.Id, Is.EqualTo(_authenticatedRequestHelper.Organization.Id));
                Assert.That(contentModule?.Description, Is.EqualTo(createContentModuleRequest.Description));
            }
        }

        [Test]
        public async Task CanGetContentModules()
        {
            var createContentModuleRequest = new ContentModuleRequest()
            {
                OrganizationId = _authenticatedRequestHelper.Organization.Id,
                Name = "Test Content Module",
                Description = "This is a test content module"
            };
            await GlobalFixtures.PostAsync("/contentmodule", createContentModuleRequest, _authenticatedRequestHelper.AuthToken, _authenticatedRequestHelper.Headers);

            var secondContentModuleRequest = new ContentModuleRequest()
            {
                OrganizationId = _authenticatedRequestHelper.Organization.Id,
                Name = "Second Test Content Module",
                Description = "This is a second test content module"
            };
            await GlobalFixtures.PostAsync("/contentmodule", secondContentModuleRequest, _authenticatedRequestHelper.AuthToken, _authenticatedRequestHelper.Headers);

            var addedContentModulesResponse = await GlobalFixtures.GetAsync("/contentmodule", _authenticatedRequestHelper.AuthToken, _authenticatedRequestHelper.Headers);
            Assert.That(addedContentModulesResponse.IsSuccessStatusCode);
            var deserializeContentModules = JsonConvert.DeserializeObject<List<ContentModule>>(await addedContentModulesResponse.Content.ReadAsStringAsync());
            Assert.That(deserializeContentModules?.Count, Is.EqualTo(2));
            Assert.That(deserializeContentModules?.Any(m => m.Name == createContentModuleRequest.Name), Is.True);
            Assert.That(deserializeContentModules?.Any(m => m.Name == secondContentModuleRequest.Name), Is.True);
        }
    }
}
