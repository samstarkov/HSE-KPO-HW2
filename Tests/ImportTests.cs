using Domain.Entities;
using Domain.ImportExport;

namespace Tests;

public class ImportTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    private string CreateTempFile(string content, string extension = ".tmp")
    {
        var path = Path.GetTempFileName() + extension;
        File.WriteAllText(path, content);
        _tempFiles.Add(path);
        return path;
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            try { File.Delete(file); } catch { }
        }
    }

    [Fact]
    public void JsonImporter_Parses_NewFormat()
    {
        var jsonData = @"{
            ""BankAccounts"": [
                { ""Id"": ""b5bc7ea7-82bf-4567-8328-7df9cee065e5"", ""Name"": ""Test"", ""Balance"": 100 }
            ],
            ""Categories"": [],
            ""Operations"": []
        }";
        var tempFile = CreateTempFile(jsonData, ".json");

        var accountRepo = new DbRepository<BankAccount>();
        var categoryRepo = new DbRepository<Category>();
        var operationRepo = new DbRepository<Operation>();
        var importer = new JsonImporter(accountRepo, categoryRepo, operationRepo);

        importer.Import(tempFile);

        Assert.Single(accountRepo.GetAllItems());
        var account = accountRepo.GetAllItems().First();
        Assert.Equal("Test", account.Name);
        Assert.Equal(100m, account.Balance);
    }

    [Fact]
    public void DataImporter_TemplateMethod_SaveCalled()
    {
        var tempFile = CreateTempFile("test data");
        var importer = new TestImporter();
        importer.Import(tempFile);
        Assert.True(importer.SaveCalled);
    }

    private class TestImporter : DataImporter
    {
        public bool SaveCalled { get; private set; }

        protected override IEnumerable<object> Parse(string content)
        {
            return new List<object> { "test object" };
        }

        protected override void Save(IEnumerable<object> items)
        {
            SaveCalled = true;
        }
    }
}
