using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Entities;
using Domain.Repositories;

namespace Domain.ImportExport;

public class CsvImporter : DataImporter
{
    private readonly IRepository<BankAccount> _accountRepo;
    private readonly IRepository<Category> _categoryRepo;
    private readonly IRepository<Operation> _operationRepo;

    public CsvImporter(
        IRepository<BankAccount> accountRepo,
        IRepository<Category> categoryRepo,
        IRepository<Operation> operationRepo)
    {
        _accountRepo = accountRepo;
        _categoryRepo = categoryRepo;
        _operationRepo = operationRepo;
    }

    protected override IEnumerable<object> Parse(string content)
    {
        var result = new List<object>();

        using var reader = new StringReader(content);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null
        });

        // Определяем тип данных по заголовкам
        if (content.Contains("Id,Name,Balance") && content.Contains("BankAccountId") == false)
        {
            // Это файл банковских счетов
            var accounts = csv.GetRecords<BankAccountCsvRecord>();
            foreach (var accountRecord in accounts)
            {
                if (Guid.TryParse(accountRecord.Id, out var accountId))
                {
                    var account = new BankAccount
                    {
                        Id = accountId,
                        Name = accountRecord.Name,
                        Balance = accountRecord.Balance
                    };
                    result.Add(account);
                }
            }
        }
        else if (content.Contains("Id,Name,Type"))
        {
            // Это файл категорий
            var categories = csv.GetRecords<CategoryCsvRecord>();
            foreach (var categoryRecord in categories)
            {
                if (Guid.TryParse(categoryRecord.Id, out var categoryId) &&
                    Enum.TryParse<TypeOfOperation>(categoryRecord.Type, out var operationType))
                {
                    var category = new Category
                    {
                        Id = categoryId,
                        Name = categoryRecord.Name,
                        Type = operationType
                    };
                    result.Add(category);
                }
            }
        }
        else if (content.Contains("Id,BankAccountId,CategoryId,Type,Amount,Date,Description"))
        {
            // Это файл операций
            var operations = csv.GetRecords<OperationCsvRecord>();
            foreach (var operationRecord in operations)
            {
                if (Guid.TryParse(operationRecord.Id, out var operationId) &&
                    Guid.TryParse(operationRecord.BankAccountId, out var accountId) &&
                    Guid.TryParse(operationRecord.CategoryId, out var categoryId) &&
                    Enum.TryParse<TypeOfOperation>(operationRecord.Type, out var operationType) &&
                    DateTime.TryParse(operationRecord.Date, out var date))
                {
                    var operation = new Operation
                    {
                        Id = operationId,
                        BankAccountId = accountId,
                        CategoryId = categoryId,
                        Type = operationType,
                        Amount = operationRecord.Amount,
                        Date = date,
                        Description = operationRecord.Description
                    };
                    result.Add(operation);
                }
            }
        }

        return result;
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

    // Классы для маппинга CSV записей
    private class BankAccountCsvRecord
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }

    private class CategoryCsvRecord
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    private class OperationCsvRecord
    {
        public string Id { get; set; } = string.Empty;
        public string BankAccountId { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Date { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}