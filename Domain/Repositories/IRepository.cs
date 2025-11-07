namespace Domain.Repositories;

public interface IRepository<T>
{
    void Add(T entity);
    void Remove(Guid entityId);
    T? Get(Guid entityId);
    IEnumerable<T> GetAllItems();
}