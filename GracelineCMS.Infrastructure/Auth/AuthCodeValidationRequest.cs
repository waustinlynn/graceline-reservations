namespace GracelineCMS.Infrastructure.Auth
{
    public class AuthCodeValidationRequest
    {
        public required string EmailAddress { get; set; }
        public required string AuthCode { get; set; }
    }
}
