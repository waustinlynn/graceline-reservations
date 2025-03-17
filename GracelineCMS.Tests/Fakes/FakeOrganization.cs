using Bogus;
using GracelineCMS.Domain.Entities;

namespace GracelineCMS.Tests.Fakes
{
    public static class FakeOrganization
    {
        public static Organization Organization
        {
            get
            {
                return new Faker<Organization>().
                    RuleFor(o => o.Name, f => f.Company.CompanyName()).Generate();
            }
        }
    }
}
