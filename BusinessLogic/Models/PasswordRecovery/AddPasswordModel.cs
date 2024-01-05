using BusinessLogic.Models.Interfaces;

namespace BusinessLogic.Models.PasswordRecovery;

public record AddPasswordModel(string Id, string Password) : IPasswordModel;
