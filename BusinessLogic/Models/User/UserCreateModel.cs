using BusinessLogic.Models.Interfaces;

namespace BusinessLogic.Models.User;

public sealed record UserCreateModel(string UserName, string Email, string Password) : IPasswordModel;