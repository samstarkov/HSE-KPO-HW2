using Domain.Entities;

namespace Domain.Factories;

public interface IDomainFactory
{
    BankAccount CreateBankAccount(string accountName, decimal startingBalance);
    Category CreateTransactionCategory(TypeOfOperation categoryType, string categoryName);
    Operation CreateFinancialOperation(TypeOfOperation operationKind, Guid targetAccountId,
        decimal operationValue, DateTime transactionDate, Guid associatedCategoryId, string? operationNotes);
}
