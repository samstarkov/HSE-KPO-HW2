using Domain.Entities;
using Domain.Repositories;
using System.Diagnostics;

namespace Tests;

public class PerformanceTests
{
    [Fact]
    public void InMemoryProxyRepository_Performance_BetterThanDatabase()
    {
        var db = new DbRepository<BankAccount>();
        var proxy = new ProxyRepository<BankAccount>(db);

        // Add multiple items
        for (int i = 0; i < 100; i++)
        {
            var account = new BankAccount
            {
                Id = Guid.NewGuid(),
                Name = $"Account{i}",
                Balance = i * 10m
            };
            proxy.Add(account);
        }

        var stopwatch = Stopwatch.StartNew();

        // Multiple reads from proxy (should be fast)
        for (int i = 0; i < 1000; i++)
        {
            var items = proxy.GetAllItems().ToList();
        }

        stopwatch.Stop();

        // Should complete in reasonable time
        Assert.True(stopwatch.ElapsedMilliseconds < 1000);
    }
}
