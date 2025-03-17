namespace GracelineCMS.Domain.Communication
{
    public class EmailCreator(DefaultEmailAddressConfig defaultConfig)
    {
        public EmailMessage CreateEmail(EmailAddressConfig addressConfig, string subject, string body)
        {
            var fromAddress = addressConfig.FromAddress ?? defaultConfig.FromAddress;
            if (fromAddress == null)
            {
                throw new ArgumentNullException("Missing From Address for EmailMessage");
            }
            var fromName = addressConfig.FromName ?? defaultConfig.FromName;
            if (fromName == null)
            {
                fromName = fromAddress;
            }

            return new EmailMessage
            {
                FromName = fromName,
                FromAddress = fromAddress,
                ToName = addressConfig.ToName ?? addressConfig.ToAddress,
                ToAddress = addressConfig.ToAddress,
                Subject = subject,
                Body = body
            };
        }
    }
}
