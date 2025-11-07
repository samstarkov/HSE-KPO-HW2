using Domain.Entities;
using Domain.Facades;
using Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Hw2;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFinTrackServices(this IServiceCollection services)
    {
        services.AddSingleton<DbRepository<BankAccount>>();
        services.AddSingleton<IRepository<BankAccount>>(sp =>
            new ProxyRepository<BankAccount>(sp.GetRequiredService<DbRepository<BankAccount>>()));

        services.AddSingleton<DbRepository<Category>>();
        services.AddSingleton<IRepository<Category>>(sp =>
            new ProxyRepository<Category>(sp.GetRequiredService<DbRepository<Category>>()));

        services.AddSingleton<DbRepository<Operation>>();
        services.AddSingleton<IRepository<Operation>>(sp =>
            new ProxyRepository<Operation>(sp.GetRequiredService<DbRepository<Operation>>()));

        services.AddSingleton<AccountFacade>();
        services.AddSingleton<CategoryFacade>();
        services.AddSingleton<OperationFacade>();
        services.AddSingleton<AnalyticFacade>();
        services.AddSingleton<Menu>();
        return services;
    }
}