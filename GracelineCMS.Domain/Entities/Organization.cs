namespace GracelineCMS.Domain.Entities
{
    public class Organization
    {
        public string Id { get; set; }
        public required string Name { get; set; }
        public Organization()
        {
            Id = Guid.NewGuid().ToString();
        }
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<ContentModule> ContentModules { get; set; } = new List<ContentModule>();
    }
}
