using Domain.Entities;
using Domain.Facades;
using Domain.ImportExport;
using Domain.Factories;
using Domain.Commands;

namespace Tests;

public class ValidationTests
{
    [Fact]
    public void DomainFactory_NegativeAmount_ThrowsException()
    {
        var factory = new DomainFactory();

        // Проверяем реальные исключения из кода
        var ex1 = Assert.Throws<ArgumentException>(() => factory.CreateBankAccount("Test", -100m));
        Assert.Contains("не может быть отрицательным", ex1.Message);

        var account = factory.CreateBankAccount("Test", 100m);
        var category = factory.CreateTransactionCategory(TypeOfOperation.Income, "TestCat");

        var ex2 = Assert.Throws<ArgumentException>(() =>
            factory.CreateFinancialOperation(TypeOfOperation.Income, account.Id, 0m, DateTime.Now, category.Id, ""));
        Assert.Contains("должна быть положительной", ex2.Message);
    }

    [Fact]
    public void ExportVisitor_InvalidDirectory_ThrowsException()
    {
        // Несуществующая директория без прав доступа или неверный путь
        var invalidPath = "Z:\\nonexistent_drive\\invalid_path\\file.json";

        Assert.ThrowsAny<Exception>(() => new DataExport(ExportFormat.Json, invalidPath));
    }

    [Fact]
    public void Repository_Operations_WithInvalidData()
    {
        var repo = new DbRepository<BankAccount>();

        // Попытка получить несуществующий элемент - должен вернуть null, а не бросить исключение
        var result = repo.Get(Guid.NewGuid());
        Assert.Null(result);

        // Удаление несуществующего элемента - не должно бросать исключение
        repo.Remove(Guid.NewGuid());

        // Работа с пустой коллекцией
        var allItems = repo.GetAllItems();
        Assert.Empty(allItems);
    }

    [Fact]
    public void OperationFacade_InvalidAccountId_CompletesWithoutError()
    {
        var accountRepo = new DbRepository<BankAccount>();
        var operationRepo = new DbRepository<Operation>();
        var operationFacade = new OperationFacade(operationRepo, accountRepo);

        // Создание операции с несуществующим accountId - не должно бросать исключение
        var operation = operationFacade.RegisterNewOperation(
            TypeOfOperation.Income,
            Guid.NewGuid(), // Несуществующий аккаунт
            100m,
            DateTime.Today,
            Guid.NewGuid(),
            "Test"
        );

        Assert.NotNull(operation);
        Assert.Single(operationRepo.GetAllItems());
    }

    [Fact]
    public void AnalyticsFacade_InvalidDateRange_ReturnsEmptyResults()
    {
        var operationRepo = new DbRepository<Operation>();
        var analytics = new AnalyticFacade(operationRepo);

        // Невалидный диапазон дат (from > to)
        var net = analytics.CalculateNetBalance(DateTime.Today.AddDays(1), DateTime.Today);
        var byCategory = analytics.GetOperationsByCategory(DateTime.Today.AddDays(1), DateTime.Today);

        Assert.Equal(0m, net);
        Assert.Empty(byCategory);
    }

    [Fact]
    public void Visitor_NullAccept_HandlesGracefully()
    {
        var account = new BankAccount { Id = Guid.NewGuid(), Name = "Test", Balance = 100m };

        // Accept с null visitor - не должно бросать исключение
        account.Accept(null);

        // Корректный вызов
        var visitor = new MockVisitor();
        account.Accept(visitor);
        Assert.True(visitor.AccountVisited);
    }

    [Fact]
    public void CreateCategoryCommand_Execution_WithValidData()
    {
        var categoryRepo = new DbRepository<Category>();
        var facade = new CategoryFacade(categoryRepo);
        var command = new CreateCategoryCommand(facade, "TestCategory", TypeOfOperation.Income);

        // Должен выполниться без исключений
        command.Execute();

        Assert.Single(categoryRepo.GetAllItems());
    }

    [Fact]
    public void ShowAnalyticsCommand_Execution_WithEmptyData()
    {
        var operationRepo = new DbRepository<Operation>();
        var analyticsFacade = new AnalyticFacade(operationRepo);
        var command = new DisplayAnalyticsCommand(analyticsFacade, DateTime.Today, DateTime.Today.AddDays(1));

        // Должен выполниться без исключений даже с пустыми данными
        command.Execute();
    }
}
