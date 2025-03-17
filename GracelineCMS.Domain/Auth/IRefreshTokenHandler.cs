namespace GracelineCMS.Domain.Auth
{
    public interface IRefreshTokenHandler
    {
        Task<bool> CanIssueRefreshTokenAsync(string email, string refreshToken);
        Task<string> CreateRefreshTokenAsync(string email);
    }
}
