using BusinessLogic.Models;
using FluentResults;
using System.Security.Claims;

namespace BusinessLogic.Interfaces;

public interface ITokenService
{
    Task<Result> AddRefreshTokenToUser(string id, string token);
    Task<Result> DeleteRefreshTokenToUser(string id, string token);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    Result<Tokens> CreateTokens(string email, string? userName = null);
}
