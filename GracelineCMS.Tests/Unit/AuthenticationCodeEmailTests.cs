using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Communication;
using GracelineCMS.Infrastructure.Communication;
using Moq;
using NUnit.Framework;

namespace GracelineCMS.Tests.Unit
{
    public class AuthenticationCodeEmailTests
    {
        [Test]
        public async Task CanSendAuthCodeViaEmail()
        {
            var defaultEmailConfig = ObjectHelpers.GetDefaultEmailAddressConfig();
            var email = "testuser@email.com";
            var emailCreator = new EmailCreator(defaultEmailConfig);
            var mockEmailClient = new Mock<IEmailClient>();
            var mockAuthenticationCode = new Mock<IAuthenticationCode>();
            mockAuthenticationCode.Setup(m => m.CreateAuthCodeAsync(email))
                .Returns(Task.FromResult("123456"));

            IAuthenticationCodeEmail authenticationCodeEmail = new AuthenticationCodeEmail(
                mockEmailClient.Object,
                emailCreator,
                mockAuthenticationCode.Object
            );
            await authenticationCodeEmail.GetCodeAndEmailUser("testuser@email.com");
            mockEmailClient.Verify(m => m.SendEmailAsync(It.IsAny<EmailMessage>()), Times.Once);
            var invocation = mockEmailClient.Invocations.Where(i => i.Method.Name == "SendEmailAsync").First();
            var emailMessage = invocation.Arguments.First() as EmailMessage;
            Assert.That(emailMessage?.ToAddress, Is.EqualTo(email));
        }
    }
}
