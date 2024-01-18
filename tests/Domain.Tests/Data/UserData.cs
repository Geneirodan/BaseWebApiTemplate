using Domain.Models.Auth;
using Infrastructure.Entities;

namespace Domain.Tests.Data;

public static class UserData
{
    internal static readonly User sampleUser = new() { Id = "1", UserName = "name", Email = "email" };

    internal static readonly IQueryable<User> usersTable = new List<User>
    {
        new() { Id = "1", UserName = "ABC", Email = "email1@gmail.com", PasswordHash = "1String!", EmailConfirmed = true },
        new() { Id = "2", UserName = "DEF", Email = "email2@gmail.com", PasswordHash = "!String1" },
        new() { Id = "3", UserName = "GHI", Email = "email3@gmail.com" }
    }.AsQueryable();

    internal static readonly IQueryable<string> validIds = usersTable.Select(x => x.Id);

    internal static readonly Tokens tokens = new() { AccessToken = "Access", RefreshToken = "Refresh" };

    internal const string ValidToken = "valid token";
    internal const string InvalidToken = "invalid token";
}
