using Domain.Repositories;

public class DbRepository<T> : IRepository<T>
{
    private readonly List<T> _dataStorage = new();

    public void Add(T entity)
    {
        // Удаляем существующий элемент с таким же ID перед добавлением
        var idProperty = typeof(T).GetProperty("Id");
        var entityId = (Guid)idProperty?.GetValue(entity)!;

        var existing = _dataStorage.FirstOrDefault(x =>
            (Guid)idProperty.GetValue(x)! == entityId);

        if (existing != null)
        {
            _dataStorage.Remove(existing);
        }

        _dataStorage.Add(entity);
    }

    public T? Get(Guid entityId)
    {
        var idProperty = typeof(T).GetProperty("Id");
        return _dataStorage.FirstOrDefault(x => (Guid)idProperty?.GetValue(x)! == entityId);
    }

    public IEnumerable<T> GetAllItems() => _dataStorage.AsReadOnly();

    public void Remove(Guid entityId)
    {
        var entityToRemove = Get(entityId);
        if (entityToRemove != null)
            _dataStorage.Remove(entityToRemove);
    }
}