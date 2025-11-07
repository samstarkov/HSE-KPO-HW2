using System.Diagnostics;

namespace Domain.Commands;

public interface ICommand { void Execute(); }

public class TimedCommandDecorator : ICommand
{
    private readonly ICommand _decoratedCommand;

    public TimedCommandDecorator(ICommand commandToDecorate)
        => _decoratedCommand = commandToDecorate;

    public void Execute()
    {
        var timer = Stopwatch.StartNew();
        _decoratedCommand.Execute();
        timer.Stop();
        Console.WriteLine($"Выполнено за {timer.ElapsedMilliseconds} мс");
    }
}