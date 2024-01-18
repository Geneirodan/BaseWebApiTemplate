using FluentResults;

namespace Domain.Interfaces;

public interface ISeeder
{
    Task<Result> SeedAsync();
}
