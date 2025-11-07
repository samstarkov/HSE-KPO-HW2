using Domain.Entities;

namespace Domain.Factories;

public class DomainFactory : IDomainFactory
{
    public BankAccount CreateBankAccount(string name, decimal initial)
    {
        ValidateInitialBalance(initial);
        return ConstructBankAccountEntity(name, initial);
    }

    public Category CreateTransactionCategory(TypeOfOperation type, string name)
    {
        return BuildCategoryEntity(type, name);
    }

    public Operation CreateFinancialOperation(TypeOfOperation type, Guid accountId, decimal amount, DateTime date, Guid categoryId, string? desc)
    {
        ValidateOperationAmount(amount);
        return GenerateOperationEntity(type, accountId, amount, date, categoryId, desc);
    }

    private void ValidateInitialBalance(decimal balance)
    {
        if (balance < 0)
            throw new ArgumentException("Начальный баланс не может быть отрицательным");
    }

    private void ValidateOperationAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Сумма операции должна быть положительной");
    }

    private BankAccount ConstructBankAccountEntity(string accountName, decimal startBalance)
    {
        return new BankAccount
        {
            Id = Guid.NewGuid(),
            Name = accountName,
            Balance = startBalance
        };
    }

    private Category BuildCategoryEntity(TypeOfOperation categoryType, string categoryName)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Type = categoryType,
            Name = categoryName
        };
    }

    private Operation GenerateOperationEntity(TypeOfOperation operationType, Guid bankAccountId,
        decimal operationAmount, DateTime operationDate, Guid operationCategoryId, string? operationDescription)
    {
        return new Operation
        {
            Id = Guid.NewGuid(),
            Type = operationType,
            BankAccountId = bankAccountId,
            CategoryId = operationCategoryId,
            Amount = operationAmount,
            Date = operationDate,
            Description = operationDescription
        };
    }
}