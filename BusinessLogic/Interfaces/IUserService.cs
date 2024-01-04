using BusinessLogic.Models;
using BusinessLogic.Models.Filters;
using FluentResults;

namespace BusinessLogic.Interfaces;

public interface IUserService
{
    Task<Result<UserViewModel>> CreateUserAsync(RegisterModel model, string role);
    Task<Result> DeleteUserAsync(string id);
    Task<Result<UserViewModel>> GetUserByIdAsync(string id);
    Task<Result<IEnumerable<UserViewModel>>> GetUsersAsync(UserFilter filter);
    Task<Result<UserViewModel>> PatchUserAsync(string id, UserPatchModel model);
}
