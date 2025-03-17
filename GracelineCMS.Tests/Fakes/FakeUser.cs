using Bogus;
using GracelineCMS.Domain.Entities;

namespace GracelineCMS.Tests.Fakes
{
    public static class FakeUser
    {
        public static User User
        {
            get
            {
                return new Faker<User>()
                    .RuleFor(u => u.EmailAddress, f => f.Internet.Email())
                    .Generate();
            }
        }
    }
}
