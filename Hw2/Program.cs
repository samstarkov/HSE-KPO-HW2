using Hw2;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    static void Main()
    {
        var services = new ServiceCollection();
        services.AddFinTrackServices();
        var provider = services.BuildServiceProvider();

        var menu = provider.GetRequiredService<Menu>();
        menu.Start();
    }
}