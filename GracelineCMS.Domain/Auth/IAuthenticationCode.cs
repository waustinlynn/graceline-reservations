namespace GracelineCMS.Domain.Auth
{
    public interface IAuthenticationCode
    {
        Task<string> CreateAuthCodeAsync(string email);
        Task DeleteExpiredCodes();
        Task ValidateCodeWithEmail(string email, string code);
    }
}
