using BankAcc.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IBankAccountRepository
{
    Task<BankAccountDto?> GetBankAccount(int id);
    Task<List<BankAccountDto>> GetAllBankAccounts();
    Task<List<BankAccountDto>> GetAllDeletedBankAccounts();
    Task<BankAccountDto> InsertBankAccountAsync(BankAccountDto bankAccountDto, int id);
    Task<BankAccountDto> UpdateBankAccountAsync(BankAccountDto bankAccountDto, int id);
    Task DeleteBankAccountAsync(int id);
    Task<List<TransactionDto>> GetTransactionReportForAllUsers();
}
