using Domain.ImportExport;
namespace Domain.Entities;

public class BankAccount : IElement
{
    private Guid _accountIdentifier;
    private string _accountName;
    private decimal _currentBalance;

    public Guid Id
    {
        get => _accountIdentifier;
        init => _accountIdentifier = value;
    }

    public string Name
    {
        get => _accountName;
        init => _accountName = value;
    }

    public decimal Balance
    {
        get => _currentBalance;
        set => _currentBalance = value;
    }

    public void Apply(Operation operation)
    {
        if (operation.Type == TypeOfOperation.Income)
        {
            _currentBalance += operation.Amount;
        }
        else
        {
            _currentBalance -= operation.Amount;
        }
    }

    public void Accept(IVisitor visitor)
    {
        visitor?.Visit(this);
    }
}