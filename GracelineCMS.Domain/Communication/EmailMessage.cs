namespace GracelineCMS.Domain.Communication
{
    public class EmailMessage
    {
        public required string ToAddress { get; set; }
        public required string ToName { get; set; }
        public string? FromAddress { get; set; }
        public string? FromName { get; set; }
        public required string Subject { get; set; }
        public required string Body { get; set; }
    }
}
