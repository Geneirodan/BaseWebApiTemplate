using System.ComponentModel.DataAnnotations;

namespace API.Requests.Recovery;

public sealed record ChangePasswordRequest
{
    [Required, DataType(DataType.Password)]
    public string OldPassword { get; init; } = null!;
    [Required, DataType(DataType.Password)]
    public string NewPassword { get; init; } = null!;
}