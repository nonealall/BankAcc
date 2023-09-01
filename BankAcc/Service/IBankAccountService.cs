using BankAcc.Dtos;
using System.Threading.Tasks;

public interface IBankAccountService
{
    Task<BankAccountDto> GetBankAccountAsync(int id);
    Task<double> Deposit(int id, double amount);
    Task<double> Withdraw(int id, double amount);
}
