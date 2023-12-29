namespace DataAccess.Interfaces;

public interface IEntity<out TKey>
{
    public TKey Id { get; }
}
