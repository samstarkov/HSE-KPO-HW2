using Domain.Entities;
using Domain.Factories;

namespace Tests;

public class FactoryTests
{
    [Fact]
    public void DomainFactory_Validations_Work()
    {
        var factory = new DomainFactory();

        // Test negative balance validation
        Assert.Throws<ArgumentException>(() => factory.CreateBankAccount("Test", -100m));

        // Test zero amount validation
        var account = factory.CreateBankAccount("Test", 0m);
        var category = factory.CreateTransactionCategory(TypeOfOperation.Income, "Salary");
        Assert.Throws<ArgumentException>(() =>
            factory.CreateFinancialOperation(TypeOfOperation.Expense, account.Id, 0m, DateTime.Now, category.Id, ""));
    }

    [Fact]
    public void DomainFactory_CreatesValidObjects()
    {
        var factory = new DomainFactory();

        var account = factory.CreateBankAccount("Test", 100m);
        var category = factory.CreateTransactionCategory(TypeOfOperation.Income, "Salary");
        var operation = factory.CreateFinancialOperation(TypeOfOperation.Income, account.Id, 50m, DateTime.Today, category.Id, "Test");

        Assert.NotNull(account);
        Assert.NotNull(category);
        Assert.NotNull(operation);
        Assert.Equal("Test", account.Name);
        Assert.Equal(100m, account.Balance);
    }

    [Fact]
    public void DomainFactory_CreateOperation_WithNegativeAmount_ThrowsException()
    {
        var factory = new DomainFactory();
        var account = factory.CreateBankAccount("Test", 100m);
        var category = factory.CreateTransactionCategory(TypeOfOperation.Income, "TestCat");

        var ex = Assert.Throws<ArgumentException>(() =>
            factory.CreateFinancialOperation(TypeOfOperation.Income, account.Id, -50m, DateTime.Now, category.Id, "Test"));

        Assert.Contains("должна быть положительной", ex.Message);
    }

    [Fact]
    public void DomainFactory_CreateValidObjects_ReturnsNotNull()
    {
        var factory = new DomainFactory();

        var account = factory.CreateBankAccount("Test", 100m);
        var category = factory.CreateTransactionCategory(TypeOfOperation.Income, "Salary");
        var operation = factory.CreateFinancialOperation(TypeOfOperation.Income, account.Id, 50m, DateTime.Today, category.Id, "Bonus");

        Assert.NotNull(account);
        Assert.NotNull(category);
        Assert.NotNull(operation);
        Assert.Equal("Test", account.Name);
        Assert.Equal("Salary", category.Name);
        Assert.Equal(50m, operation.Amount);
    }
}
