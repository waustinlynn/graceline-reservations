using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using GracelineCMS.Domain.Communication;
using MimeKit;
using System.Text;

namespace GracelineCMS.Infrastructure.Communication
{
    public class GmailClient(string encodedCredential) : IEmailClient
    {
        private ICredential AuthenticateServiceAccount()
        {
            GoogleCredential credential;
            var jsonCredential = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredential));

            credential = GoogleCredential.FromJson(jsonCredential)
                    .CreateScoped(GmailService.Scope.GmailSend)
                    .CreateWithUser("waustinlynn@gracelinesoftware.com");  // The user on whose behalf you're sending the email

            return credential.UnderlyingCredential;
        }

        // Send an email using the Gmail API
        private static async Task SendGmailAsync(GmailService service, EmailMessage emailMessage)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(emailMessage.FromName, emailMessage.FromAddress));
            mimeMessage.To.Add(new MailboxAddress(emailMessage.ToName, emailMessage.ToAddress));
            mimeMessage.Subject = emailMessage.Subject;
            mimeMessage.Body = new TextPart("plain")
            {
                Text = emailMessage.Body
            };

            var stream = new MemoryStream();
            await mimeMessage.WriteToAsync(stream);
            var encodedMessage = Convert.ToBase64String(stream.ToArray())
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");

            var message = new Message { Raw = encodedMessage };

            await service.Users.Messages.Send(message, "me").ExecuteAsync();
        }

        public async Task SendEmailAsync(EmailMessage emailMessage)
        {
            var credential = AuthenticateServiceAccount();
            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GracelineCMS",
            });

            await SendGmailAsync(service, emailMessage);
        }
    }
}

