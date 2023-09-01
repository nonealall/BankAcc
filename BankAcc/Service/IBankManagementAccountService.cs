using BankAcc.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IBankManagementAccountService
{
    Task<BankAccountDto> NewBankAccountAsync(BankAccountDto bankAccount);
    Task RemoveBankAccountAsync(int id);
    Task FireNotificationToBankManagementAsync(int accountId, double totalAmount);
    Task<int> NumBankAccAsync();
    Task<double> BankTotalMoneyAsync();
    Task<List<BankAccountDto>> ShowGoldAndBasicMembersAsync();
    Task<List<BankAccountDto>> GetDeletedBankAccountsAsync();
    Task<List<TransactionDto>> GetAllTransactionsAsync();
}
