namespace GracelineCMS.Domain.Auth
{
    public class AccessRefreshToken
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}
