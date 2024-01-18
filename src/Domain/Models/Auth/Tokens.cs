namespace Domain.Models.Auth;

public record struct Tokens(string AccessToken, string RefreshToken);
