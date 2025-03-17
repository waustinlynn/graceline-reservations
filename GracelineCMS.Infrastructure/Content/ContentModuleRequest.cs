namespace GracelineCMS.Infrastructure.Content
{
    public class ContentModuleRequest
    {
        public required string OrganizationId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
    }
}
