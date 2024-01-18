using Domain.Models;
using Domain.Models.Auth;
using Domain.Models.Filters;
using Domain.Models.User;
using FluentResults;

namespace Domain.Interfaces;

public interface IUserService
{
    Task<Result<UserViewModel>> RegisterUserAsync(RegisterModel model, string role);
    Task<Result> DeleteUserAsync(string id);
    Task<UserViewModel?> GetUserByIdAsync(string id);
    Task<Result<PaginationModel<UserViewModel>>> GetUsersAsync(UserFilter filter);
    Task<Result<UserViewModel>> PatchUserAsync(string id, UserPatchModel model);
}
