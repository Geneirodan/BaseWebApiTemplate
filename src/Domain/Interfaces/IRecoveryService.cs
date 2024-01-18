using Domain.Models.PasswordRecovery;
using FluentResults;

namespace Domain.Interfaces;

public interface IRecoveryService
{
    Task<Result> ResetPasswordAsync(ResetPasswordModel model);
    Task<Result> AddPasswordAsync(AddPasswordModel model);
    Task<Result> ChangePasswordAsync(ChangePasswordModel model);
}
