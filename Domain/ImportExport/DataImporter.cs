namespace Domain.ImportExport;

public abstract class DataImporter
{
    public void Import(string sourcePath)
    {
        var fileContent = File.ReadAllText(sourcePath);
        var processedItems = Parse(fileContent);
        Save(processedItems);
    }

    protected abstract IEnumerable<object> Parse(string content);

    protected virtual void Save(IEnumerable<object> items)
    {
        // Базовая реализация только выводит сообщение
        var itemsCount = items.Count();
        Console.WriteLine($"Импортировано элементов: {itemsCount}");
    }
}