using AutoMapper;
using BankAcc.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class BankManagementAccountService : IBankManagementAccountService
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<BankManagementAccountService> _logger;

    public BankManagementAccountService(IBankAccountRepository bankAccountRepository, IMapper mapper, ILogger<BankManagementAccountService> logger)
    {
        _bankAccountRepository = bankAccountRepository ?? throw new ArgumentNullException(nameof(bankAccountRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BankAccountDto> NewBankAccountAsync(BankAccountDto bankAccount)
    {
        try
        {
            BankAccountDto? bankDetails = await _bankAccountRepository.GetBankAccount(bankAccount.AccountNumber);
            if (bankDetails != null)
            {
                throw new InvalidOperationException("Bank account already exists.");
            }

            await _bankAccountRepository.InsertBankAccountAsync(_mapper.Map<BankAccountDto>(bankAccount), bankAccount.AccountNumber);

            return bankAccount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating a new bank account.");
            throw;
        }
    }

    public async Task RemoveBankAccountAsync(int id)
    {
        try
        {
            BankAccountDto? bankDetails = await _bankAccountRepository.GetBankAccount(id);
            if (bankDetails == null)
            {
                throw new NotFoundException("Bank account not found.");
            }

            if (bankDetails.DeletedDateTime != null)
            {
                throw new InvalidOperationException("Bank account has already been deleted.");
            }

            await _bankAccountRepository.DeleteBankAccountAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing the bank account.");
            throw;
        }
    }

    public async Task FireNotificationToBankManagementAsync(int accountId, double totalAmount)
    {
        try
        {
            BankAccountDto? bankDetails = await _bankAccountRepository.GetBankAccount(accountId);
            if (bankDetails == null)
            {
                throw new NotFoundException("Bank account not found.");
            }

            bankDetails.Type = totalAmount > 1000 ? AccountType.Gold : AccountType.Basic;

            await _bankAccountRepository.UpdateBankAccountAsync(bankDetails, accountId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while firing notification to bank management.");
            throw;
        }
    }

    public async Task<int> NumBankAccAsync()
    {
        try
        {
            List<BankAccountDto> bankAccounts = await _bankAccountRepository.GetAllBankAccounts();
            return bankAccounts.Count(b => b.DeletedDateTime == null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving the number of bank accounts.");
            throw;
        }
    }

    public async Task<double> BankTotalMoneyAsync()
    {
        try
        {
            List<BankAccountDto> bankAccounts = await _bankAccountRepository.GetAllBankAccounts();
            double totalMoney = bankAccounts.Where(b => b.DeletedDateTime == null).Sum(b => b.TotalMoney);
            return totalMoney;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calculating the bank's total money.");
            throw;
        }
    }

    public async Task<List<BankAccountDto>> ShowGoldAndBasicMembersAsync()
    {
        try
        {
            List<BankAccountDto> bankAccounts = await _bankAccountRepository.GetAllBankAccounts();
            return bankAccounts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving gold and basic members.");
            throw;
        }
    }

    public async Task<List<BankAccountDto>> GetDeletedBankAccountsAsync()
    {
        try
        {
            List<BankAccountDto> bankAccounts = await _bankAccountRepository.GetAllDeletedBankAccounts();
            return bankAccounts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving deleted bank accounts.");
            throw;
        }
    }

    public async Task<List<TransactionDto>> GetAllTransactionsAsync()
    {
        try
        {
            List<TransactionDto> transactions = await _bankAccountRepository.GetTransactionReportForAllUsers();
            return transactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all transactions.");
            throw;
        }
    }
}
