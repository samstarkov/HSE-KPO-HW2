using Domain.Entities;
using Domain.Repositories;

namespace Domain.Facades;

public class AnalyticFacade
{
    private readonly IRepository<Operation> _operationsRepository;

    public AnalyticFacade(IRepository<Operation> repository)
        => _operationsRepository = repository;

    public decimal CalculateNetBalance(DateTime startDate, DateTime endDate)
    {
        var operationsInPeriod = GetOperationsInTimeRange(startDate, endDate);
        return ComputeNetResult(operationsInPeriod);
    }

    public Dictionary<Guid, decimal> GetOperationsByCategory(DateTime startDate, DateTime endDate)
    {
        var filteredOperations = GetOperationsInTimeRange(startDate, endDate);
        return AggregateByCategory(filteredOperations);
    }

    private IEnumerable<Operation> GetOperationsInTimeRange(DateTime start, DateTime end)
    {
        return _operationsRepository.GetAllItems()
            .Where(operation => operation.Date >= start && operation.Date <= end);
    }

    private decimal ComputeNetResult(IEnumerable<Operation> operations)
    {
        return operations.Sum(operation =>
            operation.Type == TypeOfOperation.Income ? operation.Amount : -operation.Amount);
    }

    private Dictionary<Guid, decimal> AggregateByCategory(IEnumerable<Operation> operations)
    {
        return operations
            .GroupBy(operation => operation.CategoryId)
            .ToDictionary(
                group => group.Key,
                group => CalculateCategoryTotal(group));
    }

    private decimal CalculateCategoryTotal(IGrouping<Guid, Operation> categoryGroup)
    {
        return categoryGroup.Sum(operation =>
            operation.Type == TypeOfOperation.Income ? operation.Amount : -operation.Amount);
    }
}