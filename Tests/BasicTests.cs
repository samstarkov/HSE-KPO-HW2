using System.Text;
using Domain.Commands;
using Domain.Entities;
using Domain.Facades;
using Domain.ImportExport;
using Domain.Factories;
using Hw2;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public class BasicTests
{
    [Fact]
    public void AccountFacade_CreateAndList_Works()
    {
        var repo = new DbRepository<BankAccount>();
        var facade = new AccountFacade(repo);
        var account = facade.Create("TestAccount", 100m);
        var accountsList = facade.GetAllAccounts().ToList();

        Assert.Single(accountsList);
        Assert.Equal("TestAccount", accountsList[0].Name);
        Assert.Equal(100m, accountsList[0].Balance);
    }

    [Fact]
    public void DbRepository_GetNonexistent_ReturnsNull()
    {
        var repo = new DbRepository<BankAccount>();
        var result = repo.Get(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public void CategoryFacade_CreateListDelete_Works()
    {
        var repo = new DbRepository<Category>();
        var facade = new CategoryFacade(repo);

        var category = facade.Create("Entertainment", TypeOfOperation.Expense);
        Assert.Single(facade.GetAllCategories());

        facade.RemoveCategoryById(category.Id);
        Assert.Empty(facade.GetAllCategories());
    }

    [Fact]
    public void OperationFacade_CreatesOperationAndUpdatesBalance()
    {
        var accountRepo = new DbRepository<BankAccount>();
        var categoryRepo = new DbRepository<Category>();
        var operationRepo = new DbRepository<Operation>();
        var accountFacade = new AccountFacade(accountRepo);
        var categoryFacade = new CategoryFacade(categoryRepo);
        var operationFacade = new OperationFacade(operationRepo, accountRepo);

        var account = accountFacade.Create("MainAccount", 200m);
        var category = categoryFacade.Create("Salary", TypeOfOperation.Income);

        var operation = operationFacade.RegisterNewOperation(TypeOfOperation.Income, account.Id, 50m, DateTime.Today, category.Id, "Bonus");

        var updatedAccount = accountRepo.Get(account.Id);
        Assert.NotNull(updatedAccount);
        Assert.Equal(250m, updatedAccount.Balance);
        Assert.Single(operationFacade.GetAllOperations());
    }

    [Fact]
    public void AnalyticsFacade_CalculatesNetAndByCategory()
    {
        var operationRepo = new DbRepository<Operation>();
        var analytics = new AnalyticFacade(operationRepo);
        var categoryId = Guid.NewGuid();

        // Добавляем операции
        operationRepo.Add(new Operation
        {
            Id = Guid.NewGuid(),
            Type = TypeOfOperation.Income,
            Amount = 100m,
            BankAccountId = Guid.Empty,
            CategoryId = categoryId,
            Date = DateTime.Today
        });
        operationRepo.Add(new Operation
        {
            Id = Guid.NewGuid(),
            Type = TypeOfOperation.Expense,
            Amount = 40m,
            BankAccountId = Guid.Empty,
            CategoryId = categoryId,
            Date = DateTime.Today
        });

        // Проверяем чистый доход (100 - 40 = 60)
        var net = analytics.CalculateNetBalance(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));
        Assert.Equal(60m, net);

        // Проверяем сумму по категории (также 100 - 40 = 60)
        var byCategory = analytics.GetOperationsByCategory(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));
        Assert.True(byCategory.ContainsKey(categoryId));
        Assert.Equal(60m, byCategory[categoryId]);
    }
}

public class MockVisitor : IVisitor
{
    public bool AccountVisited { get; private set; }
    public bool CategoryVisited { get; private set; }
    public bool OperationVisited { get; private set; }

    public void Visit(BankAccount account) => AccountVisited = true;
    public void Visit(Category category) => CategoryVisited = true;
    public void Visit(Operation operation) => OperationVisited = true;
}
