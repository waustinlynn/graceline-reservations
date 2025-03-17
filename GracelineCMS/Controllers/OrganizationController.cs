using GracelineCMS.Domain.Entities;
using GracelineCMS.Infrastructure.Organization;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GracelineCMS.Controllers
{
    [Route("organization")]
    [ApiController]
    public class OrganizationController(IDbContextFactory<AppDbContext> dbContextFactory) : ControllerBase
    {
        [HttpGet("{id}")]
        [Authorize(Policy = "OrganizationAdmin")]
        [ProducesResponseType(typeof(Organization), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrganization([FromRoute] string id)
        {
            using (var context = await dbContextFactory.CreateDbContextAsync())
            {
                var organization = context.Organizations.Where(o => o.Id == id).FirstOrDefault();
                return Ok(organization);
            }
        }

        [HttpPost]
        [Authorize(Policy = "GlobalAdmin")]
        public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request)
        {
            using (var context = await dbContextFactory.CreateDbContextAsync())
            {
                var organization = new Organization { Name = request.Name };
                context.Organizations.Add(organization);
                await context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
