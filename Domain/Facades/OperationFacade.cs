using Domain.Entities;
using Domain.Repositories;

namespace Domain.Facades;

public class OperationFacade
{
    private readonly IRepository<Operation> _operationsStorage;
    private readonly IRepository<BankAccount> _accountsStorage;

    public OperationFacade(IRepository<Operation> operationsRepo, IRepository<BankAccount> accountsRepo)
    {
        _operationsStorage = operationsRepo;
        _accountsStorage = accountsRepo;
    }

    public Operation RegisterNewOperation(TypeOfOperation operationType, Guid accountIdentifier,
        decimal operationAmount, DateTime operationDate, Guid categoryIdentifier, string? operationDescription)
    {
        var operationData = BuildOperationEntity(operationType, accountIdentifier, operationAmount,
            operationDate, categoryIdentifier, operationDescription);

        _operationsStorage.Add(operationData);
        ProcessAccountBalanceUpdate(accountIdentifier, operationData);

        return operationData;
    }

    public IEnumerable<Operation> GetAllOperations()
        => _operationsStorage.GetAllItems();

    private Operation BuildOperationEntity(TypeOfOperation type, Guid accountId, decimal amount,
        DateTime date, Guid categoryId, string? description)
    {
        return new Operation
        {
            Id = Guid.NewGuid(),
            Type = type,
            BankAccountId = accountId,
            Amount = amount,
            Date = date,
            CategoryId = categoryId,
            Description = description
        };
    }

    private void ProcessAccountBalanceUpdate(Guid accountId, Operation operation)
    {
        var targetAccount = _accountsStorage.Get(accountId);
        if (targetAccount != null)
        {
            targetAccount.Apply(operation);
        }
    }
}