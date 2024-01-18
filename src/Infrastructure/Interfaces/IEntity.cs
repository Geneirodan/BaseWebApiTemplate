namespace Infrastructure.Interfaces;

public interface IEntity<out TKey>
{
    public TKey Id { get; }
}
