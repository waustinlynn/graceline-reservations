namespace GracelineCMS.Domain.Communication
{
    public interface IAuthenticationCodeEmail
    {
        Task GetCodeAndEmailUser(string email);
    }
}
