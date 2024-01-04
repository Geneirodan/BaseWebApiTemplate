using BusinessLogic.Models;
using DataAccess.Entities;

namespace BusinessLogin.Tests.Data;

public static class Users
{
    internal static readonly IQueryable<User> usersTable = new List<User>
    {
        new() { Id = "1", UserName = "ABC", Email = "email1@gmail.com", PasswordHash = "1String!", EmailConfirmed = true},
        new() { Id = "2", UserName = "DEF", Email = "email2@gmail.com", PasswordHash = "!String1" },
        new() { Id = "3", UserName = "GHI", Email = "email3@gmail.com", PasswordHash = "1!String!1" }
    }.AsQueryable();
    
    internal static IQueryable<string> ValidIds => usersTable.Select(x => x.Id);

    internal static readonly Tokens tokens = new() { AccessToken = "Access", RefreshToken = "Refresh" };
}
