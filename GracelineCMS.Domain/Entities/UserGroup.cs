namespace GracelineCMS.Domain.Entities
{
    public class UserGroup
    {
        public string Id { get; set; }
        public required string Name { get; set; }
        public required string OrganizationId { get; set; }
        public required Organization Organization { get; set; }
        public required User User { get; set; }
        public UserGroup()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
