using BusinessLogic.Models.Interfaces;
using BusinessLogic.Models.User;

namespace BusinessLogic.Models.Auth;

public record RegisterModel(string UserName, string Email, string Password) : IPasswordModel;
