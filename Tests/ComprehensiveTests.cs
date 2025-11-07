using Domain.Entities;
using Domain.Facades;
using Domain.ImportExport;
using Domain.Repositories;

namespace Tests;

public class ComprehensiveTests
{
    [Fact]
    public void BankAccount_ApplyOperation_UpdatesBalanceCorrectly()
    {
        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Balance = 100m
        };

        // Income operation
        var incomeOp = new Operation
        {
            Type = TypeOfOperation.Income,
            Amount = 50m
        };
        account.Apply(incomeOp);
        Assert.Equal(150m, account.Balance);

        // Expense operation
        var expenseOp = new Operation
        {
            Type = TypeOfOperation.Expense,
            Amount = 30m
        };
        account.Apply(expenseOp);
        Assert.Equal(120m, account.Balance);
    }

    [Fact]
    public void Category_AcceptVisitor_CallsCorrectMethod()
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Type = TypeOfOperation.Income
        };

        var mockVisitor = new MockVisitor();
        category.Accept(mockVisitor);

        Assert.True(mockVisitor.CategoryVisited);
        Assert.False(mockVisitor.AccountVisited);
        Assert.False(mockVisitor.OperationVisited);
    }

    [Fact]
    public void Operation_AcceptVisitor_CallsCorrectMethod()
    {
        var operation = new Operation
        {
            Id = Guid.NewGuid(),
            Type = TypeOfOperation.Income,
            Amount = 100m,
            Date = DateTime.Today
        };

        var mockVisitor = new MockVisitor();
        operation.Accept(mockVisitor);

        Assert.True(mockVisitor.OperationVisited);
        Assert.False(mockVisitor.AccountVisited);
        Assert.False(mockVisitor.CategoryVisited);
    }

    [Fact]
    public void InMemoryProxyRepository_InitializesCacheFromDatabase()
    {
        var db = new DbRepository<BankAccount>();
        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Balance = 100m
        };
        db.Add(account);

        var proxy = new ProxyRepository<BankAccount>(db);

        var cachedAccount = proxy.Get(account.Id);
        Assert.NotNull(cachedAccount);
        Assert.Equal("Test", cachedAccount.Name);
        Assert.Equal(100m, cachedAccount.Balance);
    }

    [Fact]
    public void OperationFacade_OperationWithNonexistentAccount_DoesNotThrow()
    {
        var accountRepo = new DbRepository<BankAccount>();
        var operationRepo = new DbRepository<Operation>();
        var operationFacade = new OperationFacade(operationRepo, accountRepo);

        var operation = operationFacade.RegisterNewOperation(
            TypeOfOperation.Income,
            Guid.NewGuid(),
            100m,
            DateTime.Today,
            Guid.NewGuid(),
            "Test"
        );

        Assert.NotNull(operation);
        Assert.Single(operationRepo.GetAllItems());
    }

    [Fact]
    public void AnalyticsFacade_EmptyPeriod_ReturnsZero()
    {
        var operationRepo = new DbRepository<Operation>();
        var analytics = new AnalyticFacade(operationRepo);

        // Add operation outside the period
        operationRepo.Add(new Operation
        {
            Id = Guid.NewGuid(),
            Type = TypeOfOperation.Income,
            Amount = 100m,
            BankAccountId = Guid.Empty,
            CategoryId = Guid.NewGuid(),
            Date = DateTime.Today.AddDays(-10) // Outside period
        });

        var net = analytics.CalculateNetBalance(DateTime.Today, DateTime.Today.AddDays(1));
        var byCategory = analytics.GetOperationsByCategory(DateTime.Today, DateTime.Today.AddDays(1));

        Assert.Equal(0m, net);
        Assert.Empty(byCategory);
    }

    [Fact]
    public void ExportVisitor_Json_WithAllDataTypes_CreatesValidStructure()
    {
        var tempFile = Path.GetTempFileName() + ".json";

        using (var exporter = new DataExport(ExportFormat.Json, tempFile))
        {
            exporter.Visit(new BankAccount { Id = Guid.NewGuid(), Name = "Account1", Balance = 100m });
            exporter.Visit(new BankAccount { Id = Guid.NewGuid(), Name = "Account2", Balance = 200m });
            exporter.Visit(new Category { Id = Guid.NewGuid(), Name = "IncomeCat", Type = TypeOfOperation.Income });
            exporter.Visit(new Category { Id = Guid.NewGuid(), Name = "ExpenseCat", Type = TypeOfOperation.Expense });
            exporter.Visit(new Operation
            {
                Id = Guid.NewGuid(),
                BankAccountId = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                Type = TypeOfOperation.Income,
                Amount = 50m,
                Date = DateTime.Today,
                Description = "Test"
            });
        }

        Assert.True(File.Exists(tempFile));
        var json = File.ReadAllText(tempFile);

        // Verify structure
        Assert.Contains("BankAccounts", json);
        Assert.Contains("Categories", json);
        Assert.Contains("Operations", json);
        Assert.Contains("Account1", json);
        Assert.Contains("IncomeCat", json);

        File.Delete(tempFile);
    }

    [Fact]
    public void ExportVisitor_Yaml_WithMixedData_CreatesValidStructure()
    {
        var tempFile = Path.GetTempFileName() + ".yaml";

        using (var exporter = new DataExport(ExportFormat.Yaml, tempFile))
        {
            exporter.Visit(new BankAccount { Id = Guid.NewGuid(), Name = "TestAccount", Balance = 150m });
            exporter.Visit(new Category { Id = Guid.NewGuid(), Name = "TestCategory", Type = TypeOfOperation.Expense });
        }

        Assert.True(File.Exists(tempFile));
        var yaml = File.ReadAllText(tempFile);

        Assert.Contains("bankAccounts:", yaml);
        Assert.Contains("categories:", yaml);
        Assert.Contains("TestAccount", yaml);
        Assert.Contains("TestCategory", yaml);

        File.Delete(tempFile);
    }
}
