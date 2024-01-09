using BusinessLogic.Models.PasswordRecovery;
using FluentResults;

namespace BusinessLogic.Interfaces;

public interface IPasswordRecoveryService
{
    Task<Result> ResetPasswordAsync(ResetPasswordModel model);
    Task<Result> AddPasswordAsync(AddPasswordModel model);
    Task<Result> ChangePasswordAsync(ChangePasswordModel model);
}
