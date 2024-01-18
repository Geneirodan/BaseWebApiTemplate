namespace Infrastructure.Entities;

public class Entity<TKey>
{
    public TKey Id { get; set; } = default!;
}