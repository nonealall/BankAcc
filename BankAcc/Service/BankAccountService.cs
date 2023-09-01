using AutoMapper;
using BankAcc.Dtos;
using BankAccount.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

public class BankAccountService : IBankAccountService
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IBankManagementAccountService _bankManagementService;
    private readonly IMapper _mapper;
    private readonly ILogger<BankAccountService> _logger;

    public BankAccountService(
        IBankAccountRepository bankAccountRepository,
        IBankManagementAccountService bankManagementService,
        IMapper mapper,
        ILogger<BankAccountService> logger)
    {
        _bankAccountRepository = bankAccountRepository ?? throw new ArgumentNullException(nameof(bankAccountRepository));
        _bankManagementService = bankManagementService ?? throw new ArgumentNullException(nameof(bankManagementService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BankAccountDto> GetBankAccountAsync(int id)
    {
        try
        {
            var bankAccount = await _bankAccountRepository.GetBankAccount(id);
            if (bankAccount == null)
            {
                throw new NotFoundException("Bank account not found.");
            }

            return bankAccount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving bank account.");
            throw;
        }
    }

    public async Task<double> Deposit(int id, double amount)
    {
        try
        {
            var bankDetails = await GetBankAccountAsync(id);
            double tMoney = bankDetails.TotalMoney + amount;

            TransactionDto transactionDto = new TransactionDto(
                amount,
                TransactionType.Deposit,
                id,
                DateTime.UtcNow
            );

            bankDetails.Transactions.Add(transactionDto);

            await _bankManagementService.FireNotificationToBankManagementAsync(id, Math.Round(tMoney));
            await _bankAccountRepository.UpdateBankAccountAsync(_mapper.Map<BankAccountDto>(bankDetails), id);

            return Math.Round(bankDetails.TotalMoney, 2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while depositing money.");
            throw;
        }
    }

    public async Task<double> Withdraw(int id, double amount)
    {
        try
        {
            var bankDetails = await GetBankAccountAsync(id);
            if (bankDetails.TotalMoney < amount ||
                (bankDetails.TotalMoney < 1.1 * amount && bankDetails.Type == AccountType.Basic))
            {
                throw new InsufficientFundsException("Insufficient funds to withdraw.");
            }

            if (bankDetails.Type == AccountType.Basic)
            {
                amount = 1.1 * amount;
                bankDetails.TotalMoney -= amount;
            }
            else
            {
                bankDetails.TotalMoney -= amount;
            }

            TransactionDto transactionDto = new TransactionDto(
                amount,
                TransactionType.Withdraw,
                id,
                DateTime.UtcNow
            );

            bankDetails.Transactions.Add(transactionDto);

            await _bankManagementService.FireNotificationToBankManagementAsync(id, Math.Round(bankDetails.TotalMoney));
            await _bankAccountRepository.UpdateBankAccountAsync(_mapper.Map<BankAccountDto>(bankDetails), id);

            return Math.Round(bankDetails.TotalMoney, 2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while withdrawing money.");
            throw;
        }
    }
}
