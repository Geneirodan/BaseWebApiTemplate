using BusinessLogic.Models.Auth;
using DataAccess.Entities;
using FluentResults;

namespace BusinessLogic.Interfaces;

public interface ITokenService
{
    Task<Tokens> CreateTokensAsync(User user);
    Task<Result<Tokens>> RefreshAsync(Tokens tokens);
}
