using Domain.ImportExport;
namespace Domain.Entities;

public class Operation : IElement
{
    private Guid _operationIdentifier;
    private Guid _bankAccountReference;
    private Guid _categoryReference;
    private TypeOfOperation _operationCategory;
    private decimal _operationAmount;
    private DateTime _operationDate;
    private string? _operationDescription;

    public Guid Id
    {
        get => _operationIdentifier;
        init => _operationIdentifier = value;
    }

    public Guid BankAccountId
    {
        get => _bankAccountReference;
        init => _bankAccountReference = value;
    }

    public Guid CategoryId
    {
        get => _categoryReference;
        init => _categoryReference = value;
    }

    public TypeOfOperation Type
    {
        get => _operationCategory;
        init => _operationCategory = value;
    }

    public decimal Amount
    {
        get => _operationAmount;
        init => _operationAmount = value;
    }

    public DateTime Date
    {
        get => _operationDate;
        init => _operationDate = value;
    }

    public string? Description
    {
        get => _operationDescription;
        init => _operationDescription = value;
    }

    public void Accept(IVisitor visitor)
    {
        visitor?.Visit(this);
    }
}