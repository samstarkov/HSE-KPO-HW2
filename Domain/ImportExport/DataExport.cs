using System.Text.Json;
using Domain.Entities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Domain.ImportExport;

public enum ExportFormat { Csv, Json, Yaml }

public class DataExport : IVisitor, IDisposable
{
    private readonly ExportFormat _exportType;
    private readonly string _outputPath;

    private readonly List<BankAccount> _accounts = new();
    private readonly List<Category> _categoriesList = new();
    private readonly List<Operation> _transactions = new();

    public DataExport(ExportFormat format, string path)
    {
        _exportType = format;
        _outputPath = path;

        if (_exportType == ExportFormat.Csv)
            Directory.CreateDirectory(_outputPath);
        else
        {
            var directory = Path.GetDirectoryName(_outputPath) ?? ".";
            Directory.CreateDirectory(directory);
        }
    }

    public void Visit(BankAccount account)
        => _accounts.Add(account);

    public void Visit(Category category)
        => _categoriesList.Add(category);

    public void Visit(Operation operation)
        => _transactions.Add(operation);

    public void Dispose()
    {
        switch (_exportType)
        {
            case ExportFormat.Csv:
                SaveAsCsv();
                break;
            case ExportFormat.Json:
                SaveAsJson();
                break;
            case ExportFormat.Yaml:
                SaveAsYaml();
                break;
        }
    }

    private void SaveAsCsv()
    {
        var accountsFile = Path.Combine(_outputPath, "bank_accounts.csv");
        using (var writer = new StreamWriter(accountsFile))
        {
            writer.WriteLine("Id,Name,Balance");
            foreach (var account in _accounts)
                writer.WriteLine($"{account.Id},{EscapeCsv(account.Name)},{account.Balance}");
        }

        var categoriesFile = Path.Combine(_outputPath, "categories.csv");
        using (var writer = new StreamWriter(categoriesFile))
        {
            writer.WriteLine("Id,Name,Type");
            foreach (var category in _categoriesList)
                writer.WriteLine($"{category.Id},{EscapeCsv(category.Name)},{category.Type}");
        }

        var operationsFile = Path.Combine(_outputPath, "operations.csv");
        using (var writer = new StreamWriter(operationsFile))
        {
            writer.WriteLine("Id,BankAccountId,CategoryId,Type,Amount,Date,Description");
            foreach (var operation in _transactions)
                writer.WriteLine($"{operation.Id},{operation.BankAccountId},{operation.CategoryId},{operation.Type},{operation.Amount},{operation.Date:O},{EscapeCsv(operation.Description)}");
        }
    }

    private void SaveAsJson()
    {
        var data = new
        {
            BankAccounts = _accounts,
            Categories = _categoriesList,
            Operations = _transactions
        };
        var jsonContent = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_outputPath, jsonContent);
    }

    private void SaveAsYaml()
    {
        var yamlSerializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var data = new
        {
            BankAccounts = _accounts,
            Categories = _categoriesList,
            Operations = _transactions
        };
        var yamlContent = yamlSerializer.Serialize(data);
        File.WriteAllText(_outputPath, yamlContent);
    }

    private static string EscapeCsv(string? text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return "\"" + text.Replace("\"", "\"\"") + "\"";
    }
}