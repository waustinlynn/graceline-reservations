using GracelineCMS.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GracelineCMS.Tests.Integration
{
    public class OrganizationRepositoryTests
    {
        [Test]
        public async Task CanCreateAndRetrieveOrganization()
        {
            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                var organization = FakeOrganization.Organization;

                context.Organizations.Add(organization);
                await context.SaveChangesAsync();
                var retrievedOrganizations = await context.Organizations.Where(o => o.Name == organization.Name).ToListAsync();
                Assert.That(retrievedOrganizations.Count, Is.EqualTo(1));
                var retrievedOrganization = retrievedOrganizations.FirstOrDefault();
                Assert.That(retrievedOrganization?.Name, Is.EqualTo(organization.Name));
                Assert.That(retrievedOrganization?.Id, Is.EqualTo(organization.Id));
            }
        }
    }
}
