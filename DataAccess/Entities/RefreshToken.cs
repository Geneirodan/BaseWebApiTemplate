using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities;

[Owned]
public class RefreshToken
{
    [MaxLength(sbyte.MaxValue)]
    public string Token { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}
