using FluentResults;

namespace BusinessLogic.Interfaces;

public interface IMailService
{
    Task<Result> SendEmailAsync(string email, string subject, string body);
}
