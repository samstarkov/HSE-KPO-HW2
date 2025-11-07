using Domain.Facades;
namespace Domain.Commands;

public class CreateAccountCommand : ICommand
{
    private readonly AccountFacade _accountManager;
    private readonly string _accountName;
    private readonly decimal _initialDeposit;

    public CreateAccountCommand(AccountFacade manager, string accountName, decimal initialBalance)
    {
        _accountManager = manager;
        _accountName = accountName;
        _initialDeposit = initialBalance;
    }

    public void Execute()
    {
        var newAccount = _accountManager.Create(_accountName, _initialDeposit);
        Console.WriteLine($"—чет '{newAccount.Name}' успешно создан");
    }
}