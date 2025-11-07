using Domain.ImportExport;
namespace Domain.Entities;

public class Category : IElement
{
    private Guid _categoryIdentifier;
    private string _categoryName;
    private TypeOfOperation _operationCategory;

    public Guid Id
    {
        get => _categoryIdentifier;
        init => _categoryIdentifier = value;
    }

    public string Name
    {
        get => _categoryName;
        init => _categoryName = value;
    }

    public TypeOfOperation Type
    {
        get => _operationCategory;
        init => _operationCategory = value;
    }

    public void Accept(IVisitor visitor)
    {
        visitor?.Visit(this);
    }
}