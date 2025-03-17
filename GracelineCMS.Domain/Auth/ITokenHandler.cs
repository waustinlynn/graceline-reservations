namespace GracelineCMS.Domain.Auth
{
    public interface ITokenHandler
    {
        Task<AccessRefreshToken> CreateAccessAndRefreshToken(string email);
        Task<AccessRefreshToken> RefreshToken(string refreshToken);
    }
}
