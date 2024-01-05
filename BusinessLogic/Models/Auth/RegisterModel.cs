using BusinessLogic.Models.Interfaces;

namespace BusinessLogic.Models.Auth;

public record RegisterModel(string UserName, string Email, string Password) : IPasswordModel;
