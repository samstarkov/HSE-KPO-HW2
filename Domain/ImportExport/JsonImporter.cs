using System.Text.Json;
using Domain.Entities;
using Domain.Repositories;

namespace Domain.ImportExport;

public class JsonImporter : DataImporter
{
    private readonly IRepository<BankAccount> _accountRepo;
    private readonly IRepository<Category> _categoryRepo;
    private readonly IRepository<Operation> _operationRepo;

    public JsonImporter(
        IRepository<BankAccount> accountRepo,
        IRepository<Category> categoryRepo,
        IRepository<Operation> operationRepo)
    {
        _accountRepo = accountRepo;
        _categoryRepo = categoryRepo;
        _operationRepo = operationRepo;
    }

    protected override IEnumerable<object> Parse(string jsonContent)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            var data = JsonSerializer.Deserialize<ExportData>(jsonContent, options);
            if (data != null && (data.BankAccounts != null || data.Categories != null || data.Operations != null))
            {
                return GetAllObjects(data);
            }
        }
        catch
        {

        }

        try
        {
            var oldFormatData = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonContent);
            if (oldFormatData != null)
            {
                return ConvertOldFormat(oldFormatData);
            }
        }
        catch
        {

        }

        return Enumerable.Empty<object>();
    }

    protected override void Save(IEnumerable<object> items)
    {
        foreach (var item in items)
        {
            switch (item)
            {
                case BankAccount account:
                    _accountRepo.Add(account);
                    break;
                case Category category:
                    _categoryRepo.Add(category);
                    break;
                case Operation operation:
                    _operationRepo.Add(operation);
                    break;
            }
        }
        Console.WriteLine($"Импортировано элементов: {items.Count()}");
    }

    private IEnumerable<object> GetAllObjects(ExportData data)
    {
        var result = new List<object>();
        result.AddRange(data.BankAccounts ?? Enumerable.Empty<BankAccount>());
        result.AddRange(data.Categories ?? Enumerable.Empty<Category>());
        result.AddRange(data.Operations ?? Enumerable.Empty<Operation>());
        return result;
    }

    private IEnumerable<object> ConvertOldFormat(List<Dictionary<string, object>> oldData)
    {
        var result = new List<object>();
        return result;
    }

    private class ExportData
    {
        public List<BankAccount>? BankAccounts { get; set; }
        public List<Category>? Categories { get; set; }
        public List<Operation>? Operations { get; set; }
    }
}