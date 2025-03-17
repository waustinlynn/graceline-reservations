namespace GracelineCMS.Domain.Entities
{
    public class User
    {
        public string Id { get; set; }
        public required string EmailAddress { get; set; }
        public User()
        {
            Id = Guid.NewGuid().ToString();
            RefreshTokens = new List<RefreshToken>();
            AuthCodes = new List<AuthCode>();
        }
        public ICollection<RefreshToken> RefreshTokens { get; set; }
        public ICollection<AuthCode> AuthCodes { get; set; }
        public ICollection<Organization> Organizations { get; set; } = new List<Organization>();
    }
}
