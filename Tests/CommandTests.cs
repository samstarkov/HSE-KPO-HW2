using Domain.Commands;
using Domain.Entities;
using Domain.Facades;

namespace Tests;

public class CommandTests
{
    [Fact]
    public void CreateAccountCommand_ExecutesCorrectly()
    {
        var accountRepo = new DbRepository<BankAccount>();
        var facade = new AccountFacade(accountRepo);
        var command = new CreateAccountCommand(facade, "TestAccount", 100m);

        command.Execute();

        Assert.Single(facade.GetAllAccounts());
        var account = facade.GetAllAccounts().First();
        Assert.Equal("TestAccount", account.Name);
        Assert.Equal(100m, account.Balance);
    }

    [Fact]
    public void CreateCategoryCommand_ExecutesCorrectly()
    {
        var categoryRepo = new DbRepository<Category>();
        var facade = new CategoryFacade(categoryRepo);
        var command = new CreateCategoryCommand(facade, "Food", TypeOfOperation.Expense);

        command.Execute();

        Assert.Single(facade.GetAllCategories());
        var category = facade.GetAllCategories().First();
        Assert.Equal("Food", category.Name);
        Assert.Equal(TypeOfOperation.Expense, category.Type);
    }

    [Fact]
    public void TimedCommandDecorator_WrapsExecution()
    {
        bool executed = false;
        var command = new DummyCommand(() => executed = true);
        var timedCommand = new TimedCommandDecorator(command);

        timedCommand.Execute();
        Assert.True(executed);
    }

    private class DummyCommand : ICommand
    {
        private readonly Action _action;
        public DummyCommand(Action action) => _action = action;
        public void Execute() => _action();
    }
}
