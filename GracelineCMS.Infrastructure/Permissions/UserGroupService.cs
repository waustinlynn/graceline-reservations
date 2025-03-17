using GracelineCMS.Domain.Entities;
using GracelineCMS.Domain.Permissions;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GracelineCMS.Infrastructure.Permissions
{
    public class UserGroupService(IDbContextFactory<AppDbContext> dbContextFactory) : IUserGroupService
    {
        public async Task CreateUserGroup(string userId, string organizationId, string userGroupName)
        {
            using (var context = await dbContextFactory.CreateDbContextAsync())
            {
                var user = await context.Users.Where(u => u.Id == userId).FirstAsync();
                var organization = await context.Organizations.Where(o => o.Id == organizationId).FirstAsync();
                var userGroup = new UserGroup
                {
                    User = user,
                    Organization = organization,
                    Name = userGroupName,
                    OrganizationId = organization.Id
                };
                context.UserGroups.Add(userGroup);
                await context.SaveChangesAsync();
            }
        }
    }
}
