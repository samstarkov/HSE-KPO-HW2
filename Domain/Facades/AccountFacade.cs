using Domain.Entities;
using Domain.Repositories;

namespace Domain.Facades;

public class AccountFacade
{
    private readonly IRepository<BankAccount> _accountsRepository;

    public AccountFacade(IRepository<BankAccount> repository)
        => _accountsRepository = repository;

    public BankAccount Create(string accountName, decimal initialBalance)
    {
        var newAccount = new BankAccount
        {
            Id = Guid.NewGuid(),
            Name = accountName,
            Balance = initialBalance
        };

        _accountsRepository.Add(newAccount);
        return newAccount;
    }

    public IEnumerable<BankAccount> GetAllAccounts()
        => _accountsRepository.GetAllItems();

    public void RemoveAccount(Guid accountId)
        => _accountsRepository.Remove(accountId);
}