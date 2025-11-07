using Domain.Entities;
using Domain.Facades;

namespace Domain.Commands;

public class CreateOperationCommand : ICommand
{
    private readonly OperationFacade _operationService;
    private readonly TypeOfOperation _operationType;
    private readonly Guid _targetAccountId;
    private readonly Guid _targetCategoryId;
    private readonly decimal _operationAmount;
    private readonly DateTime _operationDate;
    private readonly string _operationDetails;

    public CreateOperationCommand(OperationFacade service, TypeOfOperation type, Guid accountId,
        Guid categoryId, decimal amount, DateTime date, string description)
    {
        _operationService = service;
        _operationType = type;
        _targetAccountId = accountId;
        _targetCategoryId = categoryId;
        _operationAmount = amount;
        _operationDate = date;
        _operationDetails = description;
    }

    public void Execute()
    {
        var newOperation = _operationService.RegisterNewOperation(_operationType, _targetAccountId,
            _operationAmount, _operationDate, _targetCategoryId, _operationDetails);
        Console.WriteLine($"Операция {newOperation.Id} ({GetOperationTypeText(newOperation.Type)})" +
            $" на сумму {newOperation.Amount:C} от {newOperation.Date:dd.MM.yyyy}");
    }

    private string GetOperationTypeText(TypeOfOperation type)
    {
        return type == TypeOfOperation.Income ? "доход" : "расход";
    }
}