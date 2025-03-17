namespace GracelineCMS.Domain.Entities
{
    public class ContentModule
    {
        public string Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public required Organization Organization { get; set; }
        public ContentModule()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
