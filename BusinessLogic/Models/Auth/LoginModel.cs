using BusinessLogic.Models.Interfaces;

namespace BusinessLogic.Models.Auth;

public record LoginModel(string UserName, string Password) : IPasswordModel;
