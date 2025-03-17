using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Entities;
using GracelineCMS.Infrastructure.Content;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GracelineCMS.Controllers
{
    [Route("contentmodule")]
    [ApiController]
    [Authorize(Policy = "OrganizationAdmin")]
    public class ContentModuleController(IDbContextFactory<AppDbContext> dbContextFactory) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(List<ContentModule>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetContentModules()
        {
            if (!HttpContext.Request.Headers.TryGetValue("OrganizationId", out var organizationId))
            {
                return BadRequest("OrganizationId header not found");
            }
            var organizationIdString = organizationId.ToString();
            if (String.IsNullOrEmpty(organizationIdString))
            {
                return BadRequest("OrganizationId header not found");
            }
            using (var context = await dbContextFactory.CreateDbContextAsync())
            {
                var contentModules = await context.ContentModules.Where(cm => cm.Organization.Id == organizationIdString).ToListAsync();
                return Ok(contentModules);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateContentModule([FromBody] ContentModuleRequest request)
        {
            using (var context = await dbContextFactory.CreateDbContextAsync())
            {
                var organization = await context.Organizations.Where(o => o.Id == request.OrganizationId).FirstOrDefaultAsync();
                if (organization == null)
                {
                    return BadRequest("OrganizationId in request not found");
                }
                var contentModule = new ContentModule
                {
                    Name = request.Name,
                    Description = request.Description,
                    Organization = organization
                };
                context.ContentModules.Add(contentModule);
                await context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
