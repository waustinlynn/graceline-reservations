namespace GracelineCMS.Domain.Entities
{
    public class RefreshToken
    {
        public string Id { get; set; }
        public required string RefreshTokenValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public required User User { get; set; }
        public RefreshToken()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
            ExpiresAt = DateTime.UtcNow.AddDays(14);
        }
    }
}
