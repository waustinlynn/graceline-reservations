using GracelineCMS.Domain.Communication;
using NUnit.Framework;

namespace GracelineCMS.Tests.Unit
{
    public class EmailTests
    {
        [Test]
        public void MissingFromAddressThrowsEmailCreatorException()
        {
            var emailMessageConfig = new EmailAddressConfig()
            {
                ToAddress = "toAddress",
                ToName = "toName",
            };
            var emailCreator = new EmailCreator(new DefaultEmailAddressConfig());
            Assert.Throws<ArgumentNullException>(() =>
            {
                emailCreator.CreateEmail(emailMessageConfig, "subject", "body");
            });
        }

        [Test]
        public void MissingFromNameWillUseFromAddress()
        {
            var emailMessageConfig = new EmailAddressConfig()
            {
                ToAddress = "toAddress",
                ToName = "toName",
                FromAddress = "fromAddress"
            };
            var emailCreator = new EmailCreator(new DefaultEmailAddressConfig());
            var emailMessage = emailCreator.CreateEmail(emailMessageConfig, "subject", "body");
            Assert.That(emailMessage.FromName, Is.EqualTo(emailMessageConfig.FromAddress));
        }

        [Test]
        public void MissingFromAddressWillUseDefaultEmailAddressConfigFromAddress()
        {
            var emailMessageConfig = new EmailAddressConfig()
            {
                ToAddress = "toAddress",
                ToName = "toName",
            };
            var defaultEmailAddressConfig = new DefaultEmailAddressConfig() { FromAddress = "defaultFromAddress" };
            var emailCreator = new EmailCreator(defaultEmailAddressConfig);
            var emailMessage = emailCreator.CreateEmail(emailMessageConfig, "subject", "body");
            Assert.That(emailMessage.FromAddress, Is.EqualTo(defaultEmailAddressConfig.FromAddress));
        }
    }
}
