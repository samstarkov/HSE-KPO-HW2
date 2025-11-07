using Domain.Facades;
namespace Domain.Commands;

public class DisplayAnalyticsCommand : ICommand
{
    private readonly AnalyticFacade _analyticsService;
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;

    public DisplayAnalyticsCommand(AnalyticFacade service, DateTime start, DateTime end)
    {
        _analyticsService = service;
        _startDate = start;
        _endDate = end;
    }

    public void Execute()
    {
        var netResult = _analyticsService.CalculateNetBalance(_startDate, _endDate);
        Console.WriteLine($"„истый доход за период с {_startDate:dd.MM.yyyy} по {_endDate:dd.MM.yyyy}: {netResult:C}");

        var categoryResults = _analyticsService.GetOperationsByCategory(_startDate, _endDate);
        foreach (var categoryData in categoryResults)
            Console.WriteLine($"{categoryData.Key}: {categoryData.Value:C}");
    }
}