using Infrastructure.Entities;
using Domain.Models.Auth;
using FluentResults;

namespace Domain.Interfaces;

public interface ITokenService
{
    Task<Tokens> CreateTokensAsync(User user);
    Task<Result<Tokens>> RefreshAsync(Tokens tokens);
}
