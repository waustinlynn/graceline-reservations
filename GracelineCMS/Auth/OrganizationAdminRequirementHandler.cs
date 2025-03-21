﻿using GracelineCMS.Domain.Permissions;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GracelineCMS.Auth
{
    public class OrganizationAdminRequirementHandler(
        IHttpContextAccessor httpContextAccessor,
        IDbContextFactory<AppDbContext> dbContextFactory
    ) : AuthorizationHandler<OrganizationAdminRequirement>
    {
        protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, OrganizationAdminRequirement requirement)
        {
            var organizationId = httpContextAccessor
                .HttpContext?
                .Request
                .Headers
                .FirstOrDefault(h => h.Key == "OrganizationId").Value.ToString();

            if (String.IsNullOrEmpty(organizationId))
            {
                await Task.CompletedTask;
                context.Fail(new AuthorizationFailureReason(this, "Missing OrganizationId header value"));
                return;
            }

            var userEmail = context.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

            if (userEmail == null)
            {
                await Task.CompletedTask;
                context.Fail(new AuthorizationFailureReason(this, "Missing email claim"));
                return;
            }

            using (var dbContext = await dbContextFactory.CreateDbContextAsync())
            {
                var userGroup = await dbContext
                    .UserGroups
                    .FirstOrDefaultAsync(ug => ug.OrganizationId == organizationId && ug.User.EmailAddress == userEmail.ToLower() && ug.Name == DefaultUserGroupType.Admin.ToString());
                if (userGroup != null)
                {
                    context.Succeed(requirement);
                    return;
                }
            }

            context.Fail(new AuthorizationFailureReason(this, "User is not associated to the organization"));
        }
    }
}
