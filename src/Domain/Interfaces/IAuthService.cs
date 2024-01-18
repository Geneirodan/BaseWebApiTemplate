using Domain.Models.Auth;
using Domain.Models.User;
using FluentResults;

namespace Domain.Interfaces;

public interface IAuthService
{
    Task<Result<Tokens>> LoginAsync(LoginModel model);
    Task LogoutAsync();
    Task<Result<UserViewModel>> RegisterAsync(RegisterModel model);
    Task<Result> ConfirmEmailAsync(string userId, string token);
    Task<Result> SendEmailConfirmationAsync(string userId, string confirmUrl, string callbackUrl);
    Task<Result<Tokens>> GoogleLogin(string token);
}