namespace Domain.Repositories;

public class ProxyRepository<T> : IRepository<T>
{
    private readonly IRepository<T> _persistentStorage;
    private readonly Dictionary<Guid, T> _memoryCache = new();

    public ProxyRepository(IRepository<T> persistentRepository)
    {
        _persistentStorage = persistentRepository;
        InitializeCacheFromStorage();
    }

    public void Add(T entity)
    {
        _persistentStorage.Add(entity);
        var entityId = ExtractEntityId(entity);
        _memoryCache[entityId] = entity;
    }

    public T? Get(Guid entityId)
        => _memoryCache.ContainsKey(entityId) ? _memoryCache[entityId] : default;

    public IEnumerable<T> GetAllItems()
        => _memoryCache.Values.ToArray();

    public void Remove(Guid entityId)
    {
        _persistentStorage.Remove(entityId);
        _memoryCache.Remove(entityId);
    }

    private void InitializeCacheFromStorage()
    {
        var storedItems = _persistentStorage.GetAllItems();
        foreach (var item in storedItems)
        {
            var itemId = ExtractEntityId(item);
            _memoryCache[itemId] = item;
        }
    }

    private Guid ExtractEntityId(T entity)
    {
        var idProperty = entity?.GetType().GetProperty("Id");
        return (Guid)idProperty?.GetValue(entity)!;
    }
}