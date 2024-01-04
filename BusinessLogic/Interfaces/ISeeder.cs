using FluentResults;

namespace BusinessLogic.Interfaces;

public interface ISeeder
{
    Task<Result> SeedAsync();
}
