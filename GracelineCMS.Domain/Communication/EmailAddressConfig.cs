namespace GracelineCMS.Domain.Communication
{
    public class EmailAddressConfig
    {
        public required string ToAddress { get; set; }
        public string? ToName { get; set; }
        public string? FromAddress { get; set; }
        public string? FromName { get; set; }
    }
}
