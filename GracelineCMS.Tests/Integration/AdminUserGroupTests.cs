using GracelineCMS.Domain.Entities;
using GracelineCMS.Domain.Permissions;
using GracelineCMS.Infrastructure.Permissions;
using GracelineCMS.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GracelineCMS.Tests.Integration
{
    public class AdminUserGroupTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private IUserGroupService _userGroupService;
        private User _user;
        private Organization _organization;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        [SetUp]
        public void Setup()
        {
            _userGroupService = new UserGroupService(GlobalFixtures.DbContextFactory);
            _user = FakeUser.User;
            _organization = FakeOrganization.Organization;
        }
        [Test]
        public void CannotCreateUserGroupWithMissingOrganization()
        {
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _userGroupService.CreateUserGroup(_user.Id, _organization.Id, DefaultUserGroupType.Admin.ToString()));
        }

        [Test]
        public void CannotCreateUserGroupWithMissingUser()
        {
            var organization = FakeOrganization.Organization;
            var user = FakeUser.User;
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _userGroupService.CreateUserGroup(_user.Id, _organization.Id, DefaultUserGroupType.Admin.ToString()));
        }

        [Test]
        public async Task CannotCreateAdminUserGroupIfAlreadyExists()
        {
            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                context.Organizations.Add(_organization);
                context.Users.Add(_user);
                context.UserGroups.Add(new UserGroup()
                {
                    Name = DefaultUserGroupType.Admin.ToString(),
                    Organization = _organization,
                    OrganizationId = _organization.Id,
                    User = _user
                });
                await context.SaveChangesAsync();
            }

            Assert.ThrowsAsync<DbUpdateException>(async () => await _userGroupService.CreateUserGroup(_user.Id, _organization.Id, DefaultUserGroupType.Admin.ToString()));
        }

        [Test]
        public async Task CanCreateDefaultAdminUserGroup()
        {
            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                context.Organizations.Add(_organization);
                context.Users.Add(_user);
                await context.SaveChangesAsync();
            }

            await _userGroupService.CreateUserGroup(_user.Id, _organization.Id, DefaultUserGroupType.Admin.ToString());

            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                var userGroup = context.UserGroups.FirstOrDefault(ug => ug.Organization.Id == _organization.Id && ug.User.Id == _user.Id);
                Assert.That(userGroup, Is.Not.Null);
                Assert.That(userGroup?.Name, Is.EqualTo(DefaultUserGroupType.Admin.ToString()));
            }
        }


    }
}
