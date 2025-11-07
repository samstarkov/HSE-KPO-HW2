using Domain.Commands;
using Domain.Entities;
using Domain.Facades;

namespace Tests;

public class EdgeCaseTests
{
    [Fact]
    public void DbRepository_RemoveNonexistentItem_DoesNotThrow()
    {
        var repo = new DbRepository<BankAccount>();

        repo.Remove(Guid.NewGuid());

        Assert.Empty(repo.GetAllItems());
    }

    [Fact]
    public void DbRepository_AddDuplicateId_OverwritesPrevious()
    {
        var repo = new DbRepository<BankAccount>();
        var id = Guid.NewGuid();

        var account1 = new BankAccount { Id = id, Name = "First", Balance = 100m };
        var account2 = new BankAccount { Id = id, Name = "Second", Balance = 200m };

        repo.Add(account1);
        repo.Add(account2);

        var result = repo.Get(id);
        Assert.NotNull(result);
        Assert.Equal("Second", result.Name);
        Assert.Equal(200m, result.Balance);
    }

    [Fact]
    public void AnalyticsFacade_MultipleCategories_CorrectGrouping()
    {
        var operationRepo = new DbRepository<Operation>();
        var analytics = new AnalyticFacade(operationRepo);

        var category1 = Guid.NewGuid();
        var category2 = Guid.NewGuid();

        operationRepo.Add(new Operation { Id = Guid.NewGuid(), Type = TypeOfOperation.Income, Amount = 100m, CategoryId = category1, Date = DateTime.Today });
        operationRepo.Add(new Operation { Id = Guid.NewGuid(), Type = TypeOfOperation.Expense, Amount = 20m, CategoryId = category1, Date = DateTime.Today });

        operationRepo.Add(new Operation { Id = Guid.NewGuid(), Type = TypeOfOperation.Income, Amount = 50m, CategoryId = category2, Date = DateTime.Today });
        operationRepo.Add(new Operation { Id = Guid.NewGuid(), Type = TypeOfOperation.Expense, Amount = 10m, CategoryId = category2, Date = DateTime.Today });

        var byCategory = analytics.GetOperationsByCategory(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

        Assert.Equal(2, byCategory.Count);
        Assert.Equal(80m, byCategory[category1]);
        Assert.Equal(40m, byCategory[category2]);

        var net = analytics.CalculateNetBalance(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));
        Assert.Equal(120m, net);
    }

    [Fact]
    public void OperationFacade_ComplexScenario_AllRepositoriesUpdated()
    {
        var accountRepo = new DbRepository<BankAccount>();
        var categoryRepo = new DbRepository<Category>();
        var operationRepo = new DbRepository<Operation>();

        var accountFacade = new AccountFacade(accountRepo);
        var categoryFacade = new CategoryFacade(categoryRepo);
        var operationFacade = new OperationFacade(operationRepo, accountRepo);

        var account1 = accountFacade.Create("Account1", 100m);
        var account2 = accountFacade.Create("Account2", 200m);

        var incomeCat = categoryFacade.Create("Income", TypeOfOperation.Income);
        var expenseCat = categoryFacade.Create("Expense", TypeOfOperation.Expense);

        var op1 = operationFacade.RegisterNewOperation(TypeOfOperation.Income, account1.Id, 50m, DateTime.Today, incomeCat.Id, "Salary");
        var op2 = operationFacade.RegisterNewOperation(TypeOfOperation.Expense, account2.Id, 30m, DateTime.Today, expenseCat.Id, "Food");

        Assert.Equal(2, accountRepo.GetAllItems().Count());
        Assert.Equal(2, categoryRepo.GetAllItems().Count());
        Assert.Equal(2, operationRepo.GetAllItems().Count());

        var updatedAccount1 = accountRepo.Get(account1.Id);
        var updatedAccount2 = accountRepo.Get(account2.Id);
        Assert.Equal(150m, updatedAccount1?.Balance);
        Assert.Equal(170m, updatedAccount2?.Balance);
    }

    [Fact]
    public void CreateOperationCommand_WithValidData_ExecutesSuccessfully()
    {
        var accountRepo = new DbRepository<BankAccount>();
        var operationRepo = new DbRepository<Operation>();
        var operationFacade = new OperationFacade(operationRepo, accountRepo);

        var account = new BankAccount { Id = Guid.NewGuid(), Name = "Test", Balance = 100m };
        accountRepo.Add(account);

        var command = new CreateOperationCommand(
            operationFacade,
            TypeOfOperation.Income,
            account.Id,
            Guid.NewGuid(),
            50m,
            DateTime.Today,
            "Test operation"
        );

        // Should not throw
        command.Execute();

        Assert.Single(operationRepo.GetAllItems());
    }
}
