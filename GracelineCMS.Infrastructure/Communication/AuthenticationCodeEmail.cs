using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Communication;

namespace GracelineCMS.Infrastructure.Communication
{
    public class AuthenticationCodeEmail(IEmailClient emailClient, EmailCreator emailCreator, IAuthenticationCode authenticationCode) : IAuthenticationCodeEmail
    {
        public async Task GetCodeAndEmailUser(string email)
        {
            var code = await authenticationCode.CreateAuthCodeAsync(email);
            var emailMessage = emailCreator.CreateEmail(new EmailAddressConfig() { ToAddress = email }, "Authentication Code", $"Your authentication code is: {code}");
            await emailClient.SendEmailAsync(emailMessage);
        }
    }
}
