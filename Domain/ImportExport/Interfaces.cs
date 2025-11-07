using Domain.Entities;

namespace Domain.ImportExport;

public interface IElement
{
    void Accept(IVisitor visitor);
}

public interface IVisitor
{
    void Visit(BankAccount account);
    void Visit(Category category);
    void Visit(Operation operation);
}