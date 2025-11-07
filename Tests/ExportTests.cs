using Domain.Entities;
using Domain.ImportExport;

namespace Tests;

public class ExportTests : IDisposable
{
    private readonly List<string> _tempPaths = new();

    private string GetTempPath()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _tempPaths.Add(path);
        return path;
    }

    public void Dispose()
    {
        foreach (var path in _tempPaths)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                else if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public void ExportVisitor_Csv_WritesFiles()
    {
        var tempDir = GetTempPath();
        Directory.CreateDirectory(tempDir);

        using (var exporter = new DataExport(ExportFormat.Csv, tempDir))
        {
            exporter.Visit(new BankAccount { Id = Guid.NewGuid(), Name = "TestAccount", Balance = 123m });
            exporter.Visit(new Category { Id = Guid.NewGuid(), Name = "Food", Type = TypeOfOperation.Expense });
            exporter.Visit(new Operation
            {
                Id = Guid.NewGuid(),
                BankAccountId = Guid.Empty,
                CategoryId = Guid.Empty,
                Type = TypeOfOperation.Expense,
                Amount = 10m,
                Date = DateTime.Parse("2025-01-01"),
                Description = "Lunch"
            });
        }

        var files = Directory.GetFiles(tempDir, "*.csv");
        Assert.Equal(3, files.Length);

        var accountLines = File.ReadAllLines(Path.Combine(tempDir, "bank_accounts.csv"));
        Assert.Equal("Id,Name,Balance", accountLines[0]);
        Assert.Contains("TestAccount", accountLines[1]);
    }

    [Fact]
    public void ExportVisitor_Json_WritesFile()
    {
        var tempFile = GetTempPath() + ".json";

        using (var exporter = new DataExport(ExportFormat.Json, tempFile))
        {
            exporter.Visit(new BankAccount { Id = Guid.NewGuid(), Name = "MainAccount", Balance = 100m });
            exporter.Visit(new Category { Id = Guid.NewGuid(), Name = "Salary", Type = TypeOfOperation.Income });
        }

        Assert.True(File.Exists(tempFile));
        var json = File.ReadAllText(tempFile);
        Assert.Contains("MainAccount", json);
        Assert.Contains("Salary", json);
    }
}
