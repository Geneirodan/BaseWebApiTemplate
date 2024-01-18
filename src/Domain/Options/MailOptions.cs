namespace Domain.Options;

public class MailOptions
{
    public const string Section = "Mail";

    public string UserName { get; set; } = null!;
    public string EmailAddress { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string SmtpServer { get; set; } = null!;
    public int SmtpPort { get; set; }
}
