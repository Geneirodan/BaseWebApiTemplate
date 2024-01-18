using System.ComponentModel.DataAnnotations;

namespace API.Requests.Recovery;

public sealed record AddPasswordRequest
{
    [Required, DataType(DataType.Password)]
    public string Password { get; init; } = null!;
}