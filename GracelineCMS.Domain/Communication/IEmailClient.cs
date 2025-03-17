namespace GracelineCMS.Domain.Communication
{
    public interface IEmailClient
    {
        Task SendEmailAsync(EmailMessage emailMessage);
    }
}
